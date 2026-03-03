using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
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
    public class PostBlockService : IPostBlockService
    {
        private readonly PostBlockRepository _postBlockRepository;
        private readonly PostRepository _postRepository;

        public PostBlockService(PostBlockRepository postBlockRepository, PostRepository postRepository)
        {
            _postBlockRepository = postBlockRepository;
            _postRepository = postRepository;
        }

        public async Task<PagedResponse<PostBlockResponse>> GetAllPostBlockFilteredAsync(PostBlockFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<PostBlock>();
            var query = _postBlockRepository.GetAllPostBlockFiltered(entityFilter);

            var totalCount = await query.CountAsync();
            var entities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var items = entities.Select(ToResponse).ToList();

            return new PagedResponse<PostBlockResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<PostBlockResponse>> CreateAsync(PostBlockCreateRequest request)
        {
            var post = await _postRepository.GetByIdAsync(request.PostId);
            if (post == null)
            {
                return new ApiResponse<PostBlockResponse>
                {
                    Success = false,
                    Message = "PostId does not exist.",
                    Data = null
                };
            }

            var entity = new PostBlock
            {
                PostId = request.PostId,
                Type = request.Type.ToString(),
                Position = request.Position,
                Data = GetDataJsonString(request.Data),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var affected = await _postBlockRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = ToResponse(entity);
                return new ApiResponse<PostBlockResponse>
                {
                    Success = true,
                    Message = "Post block created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PostBlockResponse>
            {
                Success = false,
                Message = "Failed to create post block.",
                Data = null
            };
        }

        public async Task<ApiResponse<PostBlockResponse>> UpdateAsync(int id, PostBlockUpdateRequest request)
        {
            var entity = await _postBlockRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<PostBlockResponse>
                {
                    Success = false,
                    Message = "Post block not found.",
                    Data = null
                };
            }

            entity.Type = request.Type.ToString();
            entity.Position = request.Position;
            entity.Data = GetDataJsonString(request.Data);
            entity.UpdatedAt = DateTime.UtcNow;

            var affected = await _postBlockRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = ToResponse(entity);
                return new ApiResponse<PostBlockResponse>
                {
                    Success = true,
                    Message = "Post block updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PostBlockResponse>
            {
                Success = false,
                Message = "Failed to update post block.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _postBlockRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Post block not found.",
                    Data = false
                };
            }

            var removed = await _postBlockRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Post block deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete post block.",
                Data = false
            };
        }

        /// <summary>
        /// Converts JsonElement to JSON string for PostgreSQL JSONB column.
        /// </summary>
        private static string? GetDataJsonString(JsonElement? value)
        {
            if (!value.HasValue || value.Value.ValueKind == JsonValueKind.Null || value.Value.ValueKind == JsonValueKind.Undefined)
                return null;
            return value.Value.GetRawText();
        }

        /// <summary>
        /// Maps PostBlock entity to PostBlockResponse. Uses manual conversion for Data (string to JsonElement) to avoid expression tree limitations.
        /// </summary>
        private static PostBlockResponse ToResponse(PostBlock entity)
        {
            JsonElement? data = null;
            if (!string.IsNullOrEmpty(entity.Data))
            {
                data = JsonSerializer.Deserialize<JsonElement>(entity.Data);
            }
            return new PostBlockResponse
            {
                PostBlockId = entity.PostBlockId,
                PostId = entity.PostId,
                Type = entity.Type,
                Position = entity.Position,
                Data = data,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
