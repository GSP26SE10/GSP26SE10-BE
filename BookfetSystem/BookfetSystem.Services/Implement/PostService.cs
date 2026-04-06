using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class PostService : IPostService
    {
        private readonly PostRepository _postRepository;
        private readonly BlogCategoryRepository _blogCategoryRepository;
            private readonly IImageStorageService _imageStorageService;

        public PostService(
            PostRepository postRepository,
            BlogCategoryRepository blogCategoryRepository,
            IImageStorageService imageStorageService)
        {
            _postRepository = postRepository;
            _blogCategoryRepository = blogCategoryRepository;
            _imageStorageService = imageStorageService;
        }

        public async Task<PagedResponse<PostResponse>> GetAllPostFilteredAsync(PostFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Post>();
            var query = _postRepository.GetAllPostFiltered(entityFilter);

            var totalCount = await query.CountAsync();
            var entities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var items = entities.Select(ToResponse).ToList();

            return new PagedResponse<PostResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<PostResponse>> GetByIdAsync(int id)
        {
            var entity = await _postRepository.GetByIdWithBlogCategoryAsync(id);
            if (entity == null)
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "Post not found.",
                    Data = null
                };
            }

            var response = ToResponse(entity);
            return new ApiResponse<PostResponse>
            {
                Success = true,
                Message = "Post retrieved successfully.",
                Data = response
            };
        }

        public async Task<ApiResponse<PostResponse>> CreateAsync(PostCreateRequest request)
        {
            var normalizedSlug = request.Slug?.Trim().ToLowerInvariant();
            var normalizedTitle = request.Title?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedSlug))
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "Slug is required.",
                    Data = null
                };
            }

            if (string.IsNullOrWhiteSpace(normalizedTitle))
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "Title is required.",
                    Data = null
                };
            }

            var category = await _blogCategoryRepository.GetByIdAsync(request.BlogCategoryId);
            if (category == null)
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "BlogCategoryId does not exist.",
                    Data = null
                };
            }

            var slugExists = await _postRepository
                .GetAllPostFiltered(new Post { Slug = normalizedSlug })
                .AnyAsync(p => p.Slug.ToLower() == normalizedSlug);

            if (slugExists)
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "Slug already exists.",
                    Data = null
                };
            }

            var entity = new Post
            {
                Slug = normalizedSlug,
                Title = normalizedTitle,
                Excerpt = request.Excerpt?.Trim(),
                Coverimage = null,
                Status = request.Status.ToString(),
                BlogCategoryId = request.BlogCategoryId,
                PublishedAt = request.Status == PostStatus.Published ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (request.CoverImageFiles != null && request.CoverImageFiles.Count > 5)
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "Maximum 5 cover images are allowed per request.",
                    Data = null
                };
            }

            if (request.CoverImageFiles != null && request.CoverImageFiles.Count > 0)
            {
                try
                {
                    var uploadedUrls = new List<string>();
                    foreach (var file in request.CoverImageFiles.Where(f => f != null))
                    {
                        var url = await _imageStorageService.UploadImageAsync(file, CloudinaryFolder.Post);
                        uploadedUrls.Add(url);
                    }

                    entity.Coverimage = uploadedUrls.Count > 0
                        ? JsonSerializer.Serialize(uploadedUrls)
                        : null;
                }
                catch (Exception ex)
                {
                    return new ApiResponse<PostResponse>
                    {
                        Success = false,
                        Message = $"Failed to upload post cover images: {ex.Message}",
                        Data = null
                    };
                }
            }

            var affected = await _postRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = ToResponse(entity);
                return new ApiResponse<PostResponse>
                {
                    Success = true,
                    Message = "Post created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PostResponse>
            {
                Success = false,
                Message = "Failed to create post.",
                Data = null
            };
        }

        public async Task<ApiResponse<PostResponse>> UpdateAsync(int id, PostUpdateRequest request)
        {
            var entity = await _postRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "Post not found.",
                    Data = null
                };
            }

            var normalizedSlug = request.Slug?.Trim().ToLowerInvariant();
            var normalizedTitle = request.Title?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedSlug))
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "Slug is required.",
                    Data = null
                };
            }

            if (string.IsNullOrWhiteSpace(normalizedTitle))
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "Title is required.",
                    Data = null
                };
            }

            var categoryUpdate = await _blogCategoryRepository.GetByIdAsync(request.BlogCategoryId);
            if (categoryUpdate == null)
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "BlogCategoryId does not exist.",
                    Data = null
                };
            }

            var slugExists = await _postRepository
                .GetAllPostFiltered(new Post { Slug = normalizedSlug })
                .AnyAsync(p => p.PostId != id && p.Slug.ToLower() == normalizedSlug);

            if (slugExists)
            {
                return new ApiResponse<PostResponse>
                {
                    Success = false,
                    Message = "Slug already exists.",
                    Data = null
                };
            }

            entity.Slug = normalizedSlug;
            entity.Title = normalizedTitle;
            entity.Excerpt = request.Excerpt?.Trim();
            entity.Coverimage = GetCoverImageJsonString(request.CoverImage);
            entity.Status = request.Status.ToString();
            entity.BlogCategoryId = request.BlogCategoryId;
            entity.UpdatedAt = DateTime.UtcNow;

            if (request.Status == PostStatus.Published && entity.PublishedAt == null)
            {
                entity.PublishedAt = DateTime.UtcNow;
            }

            var affected = await _postRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = ToResponse(entity);
                return new ApiResponse<PostResponse>
                {
                    Success = true,
                    Message = "Post updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PostResponse>
            {
                Success = false,
                Message = "Failed to update post.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _postRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Post not found.",
                    Data = false
                };
            }

            var removed = await _postRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Post deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete post.",
                Data = false
            };
        }

        private static string? GetCoverImageJsonString(JsonElement? value)
        {
            if (!value.HasValue || value.Value.ValueKind == JsonValueKind.Null || value.Value.ValueKind == JsonValueKind.Undefined)
                return null;
            return value.Value.GetRawText();
        }

        private static PostResponse ToResponse(Post entity)
        {
            return new PostResponse
            {
                PostId = entity.PostId,
                BlogCategoryId = entity.BlogCategoryId,
                BlogCategoryName = entity.BlogCategory?.Name,
                Slug = entity.Slug,
                Title = entity.Title,
                Excerpt = entity.Excerpt,
                CoverImage = SnapshotParser.TryParseJsonToStringList(entity.Coverimage),
                Status = entity.Status,
                PublishedAt = entity.PublishedAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
