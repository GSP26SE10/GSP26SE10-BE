using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class TaskTemplateService : ITaskTemplateService
    {
        private readonly TaskTemplateRepository _taskTemplateRepository;

        public TaskTemplateService(TaskTemplateRepository taskTemplateRepository)
        {
            _taskTemplateRepository = taskTemplateRepository;
        }

        public async Task<PagedResponse<TaskTemplateResponse>> GetTaskTemplatesAsync(TaskTemplateFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<TaskTemplate>();
            var query = _taskTemplateRepository.GetAllTaskTemplateFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<TaskTemplateResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<TaskTemplateResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<TaskTemplateResponse>> GetByIdAsync(int id)
        {
            var entity = await _taskTemplateRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<TaskTemplateResponse>
                {
                    Success = false,
                    Message = "Task template not found.",
                    Data = null
                };
            }

            return new ApiResponse<TaskTemplateResponse>
            {
                Success = true,
                Message = "Task template retrieved successfully.",
                Data = entity.Adapt<TaskTemplateResponse>()
            };
        }

        public async Task<ApiResponse<TaskTemplateResponse>> CreateAsync(TaskTemplateCreateRequest request)
        {
            var normalizedName = request.TaskName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<TaskTemplateResponse>
                {
                    Success = false,
                    Message = "TaskName is required.",
                    Data = null
                };
            }

            var exists = await _taskTemplateRepository
                .GetAllTaskTemplateFiltered(new TaskTemplate { TaskName = normalizedName })
                .AnyAsync(t => t.TaskName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<TaskTemplateResponse>
                {
                    Success = false,
                    Message = "TaskName is existed.",
                    Data = null
                };
            }

            var now = DateTime.UtcNow;
            var entity = new TaskTemplate
            {
                TaskName = normalizedName,
                IsActive = request.IsActive ?? true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var affected = await _taskTemplateRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<TaskTemplateResponse>();
                return new ApiResponse<TaskTemplateResponse>
                {
                    Success = true,
                    Message = "Task template created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<TaskTemplateResponse>
            {
                Success = false,
                Message = "Failed to create task template.",
                Data = null
            };
        }

        public async Task<ApiResponse<TaskTemplateResponse>> UpdateAsync(int id, TaskTemplateUpdateRequest request)
        {
            var entity = await _taskTemplateRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<TaskTemplateResponse>
                {
                    Success = false,
                    Message = "Task template not found.",
                    Data = null
                };
            }

            var normalizedName = request.TaskName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<TaskTemplateResponse>
                {
                    Success = false,
                    Message = "TaskName is required.",
                    Data = null
                };
            }

            var exists = await _taskTemplateRepository
                .GetAllTaskTemplateFiltered(new TaskTemplate { TaskName = normalizedName })
                .AnyAsync(t => t.TaskTemplateId != id && t.TaskName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<TaskTemplateResponse>
                {
                    Success = false,
                    Message = "TaskName is existed.",
                    Data = null
                };
            }

            entity.TaskName = normalizedName;
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            var affected = await _taskTemplateRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<TaskTemplateResponse>();
                return new ApiResponse<TaskTemplateResponse>
                {
                    Success = true,
                    Message = "Task template updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<TaskTemplateResponse>
            {
                Success = false,
                Message = "Failed to update task template.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _taskTemplateRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Task template not found.",
                    Data = false
                };
            }

            if (await _taskTemplateRepository.HasRelatedDataAsync(id))
            {
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;

                var updated = await _taskTemplateRepository.UpdateAsync(entity);
                if (updated > 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Task template is in use and has been deactivated instead of deleted.",
                        Data = true
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to deactivate task template.",
                    Data = false
                };
            }

            var removed = await _taskTemplateRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Task template deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete task template.",
                Data = false
            };
        }
    }
}
