using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Hangfire;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class FeedbackServiceService : IFeedbackServiceService
    {
        private const int MaxFeedbackServiceImagesPerRequest = 3;
        private readonly FeedbackServiceRepository _feedbackServiceRepository;
        private readonly ServiceRepository _serviceRepository;
        private readonly UserRepository _userRepository;
        private readonly OrderRepository _orderRepository;
        private readonly OrderDetailRepository _orderDetailRepository;
        private readonly OrderServiceRepository _orderServiceRepository;
        private readonly IImageStorageService _imageStorageService;
        private readonly GeminiService _geminiService;
        

        public FeedbackServiceService(
            FeedbackServiceRepository feedbackServiceRepository,
            ServiceRepository serviceRepository,
            UserRepository userRepository,
            OrderRepository orderRepository,
            OrderDetailRepository orderDetailRepository,
            OrderServiceRepository orderServiceRepository,
            IImageStorageService imageStorageService,
            GeminiService geminiService)
        {
            _feedbackServiceRepository = feedbackServiceRepository;
            _serviceRepository = serviceRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _orderServiceRepository = orderServiceRepository;
            _imageStorageService = imageStorageService;
            _geminiService = geminiService;
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
            // 1. Kiểm tra các thực thể (Validation)
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return new ApiResponse<FeedbackServiceResponse> { Success = false, Message = "Order not found.", Data = null };
            }

            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
            {
                return new ApiResponse<FeedbackServiceResponse> { Success = false, Message = "Service not found.", Data = null };
            }

            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<FeedbackServiceResponse> { Success = false, Message = "Order detail not found.", Data = null };
            }

            if (orderDetail.OrderId != request.OrderId)
            {
                return new ApiResponse<FeedbackServiceResponse> { Success = false, Message = "Order detail does not belong to the specified order.", Data = null };
            }

            var hasServiceInOrderDetail = await _orderServiceRepository
                .GetAllOrderServiceFiltered(new OrderService
                {
                    OrderDetailId = request.OrderDetailId,
                    ServiceId = request.ServiceId
                })
                .AnyAsync();

            if (!hasServiceInOrderDetail)
            {
                return new ApiResponse<FeedbackServiceResponse> { Success = false, Message = "Service does not belong to the specified order detail.", Data = null };
            }

            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<FeedbackServiceResponse> { Success = false, Message = "Customer not found.", Data = null };
            }

            // 2. Khởi tạo Entity
            var entity = new FeedbackService
            {
                OrderId = request.OrderId,
                OrderDetailId = request.OrderDetailId,
                ServiceId = request.ServiceId,
                CustomerId = request.CustomerId,
                Rating = request.Rating,
                Comment = request.Comment?.Trim(),
                Status = FeedbackServiceStatus.ACTIVE.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            // 3. Xử lý upload hình ảnh
            try
            {
                var files = NormalizeFeedbackServiceImageFiles(request.ImgFiles);
                if (files.Count > MaxFeedbackServiceImagesPerRequest)
                {
                    return new ApiResponse<FeedbackServiceResponse>
                    {
                        Success = false,
                        Message = $"You can upload up to {MaxFeedbackServiceImagesPerRequest} images at once.",
                        Data = null
                    };
                }

                if (files.Count > 0)
                {
                    var uploadedUrls = new List<string>(files.Count);
                    foreach (var file in files)
                    {
                        var uploadedUrl = await _imageStorageService.UploadImageAsync(file, CloudinaryFolder.FeedbackService);
                        uploadedUrls.Add(uploadedUrl);
                    }
                    entity.Img = JsonSerializer.Serialize(uploadedUrls);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = false,
                    Message = $"Failed to upload feedback service images: {ex.Message}",
                    Data = null
                };
            }

            // 4. Lưu Feedback vào Database
            var affected = await _feedbackServiceRepository.CreateAsync(entity);

            if (affected > 0)
            {
                // CHIẾN LƯỢC MỚI: Luôn chạy AI Summary ngay lập tức sau mỗi feedback
                // Bỏ qua việc đếm số lượng feedback để dữ liệu luôn được cập nhật mới nhất
                BackgroundJob.Enqueue<IFeedbackServiceService>(s => s.ProcessAiServiceSummaryAsync(request.ServiceId));

                // 5. Query lại dữ liệu để trả về đầy đủ thông tin cho Client
                var responseData = await _feedbackServiceRepository
                    .GetAllFeedbackServiceFiltered(new FeedbackService { FeedbackServiceId = entity.FeedbackServiceId })
                    .ProjectToType<FeedbackServiceResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = true,
                    Message = "Feedback service created successfully.",
                    Data = responseData
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

            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return new ApiResponse<FeedbackServiceResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            entity.OrderId = request.OrderId;
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
                // Lấy thông tin service để check summary cũ
                var serviceEntities = await _serviceRepository.GetByIdAsync(request.ServiceId);

                // Đếm tổng số feedback
                var totalFeedback = await _feedbackServiceRepository
                    .GetAllFeedbackServiceFiltered(new FeedbackService { ServiceId = request.ServiceId })
                    .CountAsync();

                // CHIẾN LƯỢC: Chạy khi chưa có summary HOẶC mỗi 5 feedback
                if (serviceEntities != null && (string.IsNullOrEmpty(serviceEntities.AisServiceSummary) || totalFeedback % 5 == 0))
                {
                    BackgroundJob.Enqueue<IFeedbackServiceService>(s => s.ProcessAiServiceSummaryAsync(request.ServiceId));
                }
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

        private static List<Microsoft.AspNetCore.Http.IFormFile> NormalizeFeedbackServiceImageFiles(
            List<Microsoft.AspNetCore.Http.IFormFile>? fileList)
        {
            var files = new List<Microsoft.AspNetCore.Http.IFormFile>();

            if (fileList != null && fileList.Count > 0)
            {
                files.AddRange(fileList.Where(f => f != null && f.Length > 0));
            }

            return files;
        }
        public async Task ProcessAiServiceSummaryAsync(int serviceId)
        {
            await Task.Delay(10000);
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null) return;

            var recentComments = await _feedbackServiceRepository
                .GetAllFeedbackServiceFiltered(new FeedbackService { ServiceId = serviceId })
                .OrderByDescending(f => f.CreatedAt)
                .Where(f => !string.IsNullOrWhiteSpace(f.Comment))
                .Select(f => f.Comment)
                .Take(15)
                .ToListAsync();

            if (recentComments.Any())
            {
                try
                {
                    // Gọi AI với 3 tham số: Tên, Feedback mới, Summary cũ
                    var summary = await _geminiService.SummarizeServiceFeedbackAsync(
                        service.ServiceName,
                        recentComments,
                        service.AisServiceSummary
                    );

                    if (!string.IsNullOrEmpty(summary))
                    {
                        // LƯU VÀO BẢNG SERVICE (Cột ais_service_summary)
                        service.AisServiceSummary = summary.Trim();
                        await _serviceRepository.UpdateAsync(service);
                        Console.WriteLine($"[AI Service Summary Success] Service: {service.ServiceName}");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi tóm tắt AI cho Service {serviceId}: {ex.Message}");
                }
            }
        }
    }
}
