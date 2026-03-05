using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
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
    public class FeedbackServiceService : IFeedbackServiceService
    {
        private readonly FeedbackServiceRepository _feedbackServiceRepository;
        private readonly ServiceRepository _serviceRepository;
        private readonly UserRepository _userRepository;

        public FeedbackServiceService(
            FeedbackServiceRepository feedbackServiceRepository,
            ServiceRepository serviceRepository,
            UserRepository userRepository)
        {
            _feedbackServiceRepository = feedbackServiceRepository;
            _serviceRepository = serviceRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResponse<FeedbackServiceResponse>> GetAllFeedbackServiceFilteredAsync(FeedbackServiceFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<FeedbackService>();
            entityFilter.Status = request.Status?.ToString();
            entityFilter.Rating = request.Rating ?? 0;

            var query = _feedbackServiceRepository.GetAllFeedbackServiceFiltered(entityFilter);
            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<FeedbackServiceResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<FeedbackServiceResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<FeedbackServiceResponse>> CreateAsync(FeedbackServiceCreateRequest request)
        {
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
            {
                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = false,
                    Message = "Service not found.",
                    Data = null
                };
            }

            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null
                };
            }

            var entity = new FeedbackService
            {
                ServiceId = request.ServiceId,
                CustomerId = request.CustomerId,
                Rating = request.Rating,
                Comment = request.Comment?.Trim(),
                Status = FeedbackServiceStatus.ACTIVE.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            var affected = await _feedbackServiceRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = await _feedbackServiceRepository
                    .GetAllFeedbackServiceFiltered(new FeedbackService { FeedbackServiceId = entity.FeedbackServiceId })
                    .ProjectToType<FeedbackServiceResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = true,
                    Message = "Feedback service created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<FeedbackServiceResponse>
            {
                Success = false,
                Message = "Failed to create feedback service.",
                Data = null
            };
        }

        public async Task<ApiResponse<FeedbackServiceResponse>> UpdateAsync(int id, FeedbackServiceUpdateRequest request)
        {
            var entity = await _feedbackServiceRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = false,
                    Message = "Feedback service not found.",
                    Data = null
                };
            }

            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
            {
                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = false,
                    Message = "Service not found.",
                    Data = null
                };
            }

            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null
                };
            }

            entity.ServiceId = request.ServiceId;
            entity.CustomerId = request.CustomerId;
            entity.Rating = request.Rating;
            entity.Comment = request.Comment?.Trim();
            if (request.Status != null)
            {
                entity.Status = request.Status.ToString();
            }

            var affected = await _feedbackServiceRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = await _feedbackServiceRepository
                    .GetAllFeedbackServiceFiltered(new FeedbackService { FeedbackServiceId = entity.FeedbackServiceId })
                    .ProjectToType<FeedbackServiceResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = true,
                    Message = "Feedback service updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<FeedbackServiceResponse>
            {
                Success = false,
                Message = "Failed to update feedback service.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _feedbackServiceRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Feedback service not found.",
                    Data = false
                };
            }

            var removed = await _feedbackServiceRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Feedback service deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete feedback service.",
                Data = false
            };
        }
    }
}
