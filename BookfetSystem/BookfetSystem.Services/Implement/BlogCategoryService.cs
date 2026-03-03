using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class BlogCategoryService : IBlogCategoryService
    {
        private readonly BlogCategoryRepository _blogCategoryRepository;

        public BlogCategoryService(BlogCategoryRepository blogCategoryRepository)
        {
            _blogCategoryRepository = blogCategoryRepository;
        }

        public async Task<PagedResponse<BlogCategoryResponse>> GetAllBlogCategoryFilteredAsync(BlogCategoryFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<BlogCategory>();
            var query = _blogCategoryRepository.GetAllBlogCategoryFiltered(entityFilter);

            var totalCount = await query.CountAsync();
            var items = await query
                .ProjectToType<BlogCategoryResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<BlogCategoryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<BlogCategoryResponse>> CreateAsync(BlogCategoryCreateRequest request)
        {
            var normalizedName = request.Name?.Trim();
            var normalizedSlug = request.Slug?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<BlogCategoryResponse>
                {
                    Success = false,
                    Message = "Name is required.",
                    Data = null
                };
            }

            if (string.IsNullOrWhiteSpace(normalizedSlug))
            {
                return new ApiResponse<BlogCategoryResponse>
                {
                    Success = false,
                    Message = "Slug is required.",
                    Data = null
                };
            }

            var slugExists = await _blogCategoryRepository
                .GetAllBlogCategoryFiltered(new BlogCategory { Slug = normalizedSlug })
                .AnyAsync(bc => bc.Slug.ToLower() == normalizedSlug);

            if (slugExists)
            {
                return new ApiResponse<BlogCategoryResponse>
                {
                    Success = false,
                    Message = "Slug already exists.",
                    Data = null
                };
            }

            var entity = new BlogCategory
            {
                Name = normalizedName,
                Slug = normalizedSlug
            };

            var affected = await _blogCategoryRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<BlogCategoryResponse>();
                return new ApiResponse<BlogCategoryResponse>
                {
                    Success = true,
                    Message = "Blog category created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<BlogCategoryResponse>
            {
                Success = false,
                Message = "Failed to create blog category.",
                Data = null
            };
        }

        public async Task<ApiResponse<BlogCategoryResponse>> UpdateAsync(int id, BlogCategoryUpdateRequest request)
        {
            var entity = await _blogCategoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<BlogCategoryResponse>
                {
                    Success = false,
                    Message = "Blog category not found.",
                    Data = null
                };
            }

            var normalizedName = request.Name?.Trim();
            var normalizedSlug = request.Slug?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<BlogCategoryResponse>
                {
                    Success = false,
                    Message = "Name is required.",
                    Data = null
                };
            }

            if (string.IsNullOrWhiteSpace(normalizedSlug))
            {
                return new ApiResponse<BlogCategoryResponse>
                {
                    Success = false,
                    Message = "Slug is required.",
                    Data = null
                };
            }

            var slugExists = await _blogCategoryRepository
                .GetAllBlogCategoryFiltered(new BlogCategory { Slug = normalizedSlug })
                .AnyAsync(bc => bc.BlogCategoryId != id && bc.Slug.ToLower() == normalizedSlug);

            if (slugExists)
            {
                return new ApiResponse<BlogCategoryResponse>
                {
                    Success = false,
                    Message = "Slug already exists.",
                    Data = null
                };
            }

            entity.Name = normalizedName;
            entity.Slug = normalizedSlug;

            var affected = await _blogCategoryRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<BlogCategoryResponse>();
                return new ApiResponse<BlogCategoryResponse>
                {
                    Success = true,
                    Message = "Blog category updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<BlogCategoryResponse>
            {
                Success = false,
                Message = "Failed to update blog category.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _blogCategoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Blog category not found.",
                    Data = false
                };
            }

            if (await _blogCategoryRepository.HasRelatedDataAsync(id))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete blog category because it is being used by posts.",
                    Data = false
                };
            }

            var removed = await _blogCategoryRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Blog category deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete blog category.",
                Data = false
            };
        }
    }
}
