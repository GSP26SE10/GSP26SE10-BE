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
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class FeedbackMenuService : IFeedbackMenuService
    {
        private const int MaxFeedbackMenuImagesPerRequest = 3;
        private readonly FeedbackMenuRepository _feedbackMenuRepository;
        private readonly MenuRepository _menuRepository;
        private readonly UserRepository _userRepository;
        private readonly OrderRepository _orderRepository;
        private readonly OrderDetailRepository _orderDetailRepository;
        private readonly IImageStorageService _imageStorageService;

        public FeedbackMenuService(
            FeedbackMenuRepository feedbackMenuRepository,
            MenuRepository menuRepository,
            UserRepository userRepository,
            OrderRepository orderRepository,
            OrderDetailRepository orderDetailRepository,
            IImageStorageService imageStorageService)
        {
            _feedbackMenuRepository = feedbackMenuRepository;
            _menuRepository = menuRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _imageStorageService = imageStorageService;
        }

        public async Task<PagedResponse<FeedbackMenuResponse>> GetAllFeedbackMenuFilteredAsync(FeedbackMenuFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<FeedbackMenu>();
            entityFilter.Status = request.Status?.ToString();
            entityFilter.Rating = request.Rating ?? 0;

            var query = _feedbackMenuRepository.GetAllFeedbackMenuFiltered(entityFilter);
            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<FeedbackMenuResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<FeedbackMenuResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<FeedbackMenuResponse>> CreateAsync(FeedbackMenuCreateRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            var menu = await _menuRepository.GetByIdAsync(request.MenuId);
            if (menu == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = null
                };
            }

            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Order detail not found.",
                    Data = null
                };
            }

            if (orderDetail.OrderId != request.OrderId)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Order detail does not belong to the specified order.",
                    Data = null
                };
            }

            if (orderDetail.MenuId != request.MenuId)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Menu does not belong to the specified order detail.",
                    Data = null
                };
            }

            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null
                };
            }

            var entity = new FeedbackMenu
            {
                OrderId = request.OrderId,
                OrderDetailId = request.OrderDetailId,
                MenuId = request.MenuId,
                CustomerId = request.CustomerId,
                Rating = request.Rating,
                Comment = request.Comment?.Trim(),
                Status = FeedbackMenuStatus.ACTIVE.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var files = NormalizeFeedbackMenuImageFiles(request.ImgFiles);
                if (files.Count > MaxFeedbackMenuImagesPerRequest)
                {
                    return new ApiResponse<FeedbackMenuResponse>
                    {
                        Success = false,
                        Message = $"You can upload up to {MaxFeedbackMenuImagesPerRequest} images at once.",
                        Data = null
                    };
                }

                if (files.Count > 0)
                {
                    var uploadedUrls = new List<string>(files.Count);
                    foreach (var file in files)
                    {
                        var uploadedUrl = await _imageStorageService.UploadImageAsync(file, CloudinaryFolder.FeedbackMenu);
                        uploadedUrls.Add(uploadedUrl);
                    }

                    entity.Img = JsonSerializer.Serialize(uploadedUrls);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = $"Failed to upload feedback menu images: {ex.Message}",
                    Data = null
                };
            }

            var affected = await _feedbackMenuRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = await _feedbackMenuRepository
                    .GetAllFeedbackMenuFiltered(new FeedbackMenu { FeedbackMenuId = entity.FeedbackMenuId })
                    .ProjectToType<FeedbackMenuResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = true,
                    Message = "Feedback menu created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<FeedbackMenuResponse>
            {
                Success = false,
                Message = "Failed to create feedback menu.",
                Data = null
            };
        }

        public async Task<ApiResponse<FeedbackMenuResponse>> UpdateAsync(int id, FeedbackMenuUpdateRequest request)
        {
            var entity = await _feedbackMenuRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Feedback menu not found.",
                    Data = null
                };
            }

            var menu = await _menuRepository.GetByIdAsync(request.MenuId);
            if (menu == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = null
                };
            }

            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null
                };
            }

            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            entity.OrderId = request.OrderId;
            entity.MenuId = request.MenuId;
            entity.CustomerId = request.CustomerId;
            entity.Rating = request.Rating;
            entity.Comment = request.Comment?.Trim();
            if (request.Status != null)
            {
                entity.Status = request.Status.ToString();
            }

            var affected = await _feedbackMenuRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = await _feedbackMenuRepository
                    .GetAllFeedbackMenuFiltered(new FeedbackMenu { FeedbackMenuId = entity.FeedbackMenuId })
                    .ProjectToType<FeedbackMenuResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = true,
                    Message = "Feedback menu updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<FeedbackMenuResponse>
            {
                Success = false,
                Message = "Failed to update feedback menu.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _feedbackMenuRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Feedback menu not found.",
                    Data = false
                };
            }

            var removed = await _feedbackMenuRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Feedback menu deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete feedback menu.",
                Data = false
            };
        }

        private static List<Microsoft.AspNetCore.Http.IFormFile> NormalizeFeedbackMenuImageFiles(
            List<Microsoft.AspNetCore.Http.IFormFile>? fileList)
        {
            var files = new List<Microsoft.AspNetCore.Http.IFormFile>();

            if (fileList != null && fileList.Count > 0)
            {
                files.AddRange(fileList.Where(f => f != null && f.Length > 0));
            }

            return files;
        }
    }
}
