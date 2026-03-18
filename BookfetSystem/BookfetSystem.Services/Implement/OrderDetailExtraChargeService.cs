using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class OrderDetailExtraChargeService : IOrderDetailExtraChargeService
    {
        private const int MaxExtraChargeImagesPerRequest = 5;

        private readonly OrderDetailExtraChargeRepository _orderDetailExtraChargeRepository;
        private readonly OrderDetailRepository _orderDetailRepository;
        private readonly ExtraChargeCatalogRepository _extraChargeCatalogRepository;
        private readonly StaffGroupRepository _staffGroupRepository;
        private readonly IImageStorageService _imageStorageService;

        public OrderDetailExtraChargeService(
            OrderDetailExtraChargeRepository orderDetailExtraChargeRepository,
            OrderDetailRepository orderDetailRepository,
            ExtraChargeCatalogRepository extraChargeCatalogRepository,
            StaffGroupRepository staffGroupRepository,
            IImageStorageService imageStorageService)
        {
            _orderDetailExtraChargeRepository = orderDetailExtraChargeRepository;
            _orderDetailRepository = orderDetailRepository;
            _extraChargeCatalogRepository = extraChargeCatalogRepository;
            _staffGroupRepository = staffGroupRepository;
            _imageStorageService = imageStorageService;
        }

        public async Task<ApiResponse<OrderDetailExtraChargeResponse>> CreateAsync(OrderDetailExtraChargeCreateRequest request, int leaderId)
        {
            var staffGroup = await _staffGroupRepository
                .GetAllStaffGroupFiltered(new StaffGroup { LeaderId = leaderId })
                .FirstOrDefaultAsync(x => x.Status == "ACTIVE");

            if (staffGroup == null)
            {
                return new ApiResponse<OrderDetailExtraChargeResponse>
                {
                    Success = false,
                    Message = "Leader does not have an active staff group.",
                    Data = null
                };
            }

            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<OrderDetailExtraChargeResponse>
                {
                    Success = false,
                    Message = "Order detail not found.",
                    Data = null
                };
            }

            if (orderDetail.StaffGroupId != staffGroup.StaffGroupId)
            {
                return new ApiResponse<OrderDetailExtraChargeResponse>
                {
                    Success = false,
                    Message = "Order detail does not belong to your staff group.",
                    Data = null
                };
            }

            var orderDetailStatus = orderDetail.Status ?? string.Empty;
            var isAllowedStatus =
                string.Equals(orderDetailStatus, OrderStatus.IN_PROGRESS.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(orderDetailStatus, OrderStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase);

            if (!isAllowedStatus)
            {
                return new ApiResponse<OrderDetailExtraChargeResponse>
                {
                    Success = false,
                    Message = "Extra charge can only be created when order detail is IN_PROGRESS or COMPLETED.",
                    Data = null
                };
            }

            var catalog = await _extraChargeCatalogRepository.GetByIdAsync(request.ExtraChargeCatalogId);
            if (catalog == null)
            {
                return new ApiResponse<OrderDetailExtraChargeResponse>
                {
                    Success = false,
                    Message = "Extra charge catalog not found.",
                    Data = null
                };
            }

            var unitPrice = catalog.UnitPrice ?? 0m;
            var quantity = request.Quantity;
            var totalAmount = unitPrice * quantity;

            var entity = new OrderDetailExtraCharge
            {
                OrderDetailId = request.OrderDetailId,
                ExtraChargeCatalogId = request.ExtraChargeCatalogId,
                ChargeType = catalog.ChargeType,
                Title = catalog.Title,
                Description = catalog.Description,
                Unit = catalog.Unit,
                UnitPrice = unitPrice,
                Quantity = quantity,
                TotalAmount = totalAmount,
                Status = "ACTIVE",
                CreateBy = leaderId,
                IncurredAt = request.IncurredAt ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Note = request.Note?.Trim()
            };

            try
            {
                var files = NormalizeImageFiles(request.ImageFiles);
                if (files.Count > MaxExtraChargeImagesPerRequest)
                {
                    return new ApiResponse<OrderDetailExtraChargeResponse>
                    {
                        Success = false,
                        Message = $"You can upload up to {MaxExtraChargeImagesPerRequest} images at once.",
                        Data = null
                    };
                }

                if (files.Count > 0)
                {
                    var uploadedUrls = new List<string>(files.Count);
                    foreach (var file in files)
                    {
                        var uploadedUrl = await _imageStorageService.UploadImageAsync(file, CloudinaryFolder.ExtraCharge);
                        uploadedUrls.Add(uploadedUrl);
                    }

                    entity.Image = JsonSerializer.Serialize(uploadedUrls);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<OrderDetailExtraChargeResponse>
                {
                    Success = false,
                    Message = $"Failed to upload extra charge images: {ex.Message}",
                    Data = null
                };
            }

            var affected = await _orderDetailExtraChargeRepository.CreateAsync(entity);
            if (affected <= 0)
            {
                return new ApiResponse<OrderDetailExtraChargeResponse>
                {
                    Success = false,
                    Message = "Failed to create order detail extra charge.",
                    Data = null
                };
            }

            var response = new OrderDetailExtraChargeResponse
            {
                OrderDetailExtraChargeId = entity.OrderDetailExtraChargeId,
                OrderDetailId = entity.OrderDetailId,
                ExtraChargeCatalogId = entity.ExtraChargeCatalogId,
                ChargeType = entity.ChargeType,
                Title = entity.Title,
                Description = entity.Description,
                Unit = entity.Unit,
                UnitPrice = entity.UnitPrice,
                Quantity = entity.Quantity,
                TotalAmount = entity.TotalAmount,
                Status = entity.Status,
                CreateBy = entity.CreateBy,
                IncurredAt = entity.IncurredAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Image = SnapshotParser.TryParseJsonToObject(entity.Image),
                Note = entity.Note
            };

            return new ApiResponse<OrderDetailExtraChargeResponse>
            {
                Success = true,
                Message = "Order detail extra charge created successfully.",
                Data = response
            };
        }

        public async Task<List<ExtraChargeCatalogResponse>> GetActiveCatalogAsync()
        {
            var items = await _extraChargeCatalogRepository
                .GetAllFiltered(new ExtraChargeCatalog(), activeOnly: true)
                .Select(x => new ExtraChargeCatalogResponse
                {
                    ExtraChargeCatalogId = x.ExtraChargeCatalogId,
                    ChargeType = x.ChargeType,
                    Title = x.Title,
                    Description = x.Description,
                    Unit = x.Unit,
                    UnitPrice = x.UnitPrice,
                    Status = x.Status
                })
                .ToListAsync();

            return items;
        }

        public async Task<List<OrderDetailExtraChargeResponse>> GetByOrderIdAsync(int orderId)
        {
            var items = await _orderDetailExtraChargeRepository
                .GetByOrderId(orderId)
                .Select(x => new OrderDetailExtraChargeResponse
                {
                    OrderDetailExtraChargeId = x.OrderDetailExtraChargeId,
                    OrderDetailId = x.OrderDetailId,
                    ExtraChargeCatalogId = x.ExtraChargeCatalogId,
                    ChargeType = x.ChargeType,
                    Title = x.Title,
                    Description = x.Description,
                    Unit = x.Unit,
                    UnitPrice = x.UnitPrice,
                    Quantity = x.Quantity,
                    TotalAmount = x.TotalAmount,
                    Status = x.Status,
                    CreateBy = x.CreateBy,
                    CreatorName = x.CreateByNavigation != null ? x.CreateByNavigation.FullName : null,
                    IncurredAt = x.IncurredAt,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Image = SnapshotParser.TryParseJsonToObject(x.Image),
                    Note = x.Note
                })
                .ToListAsync();

            return items;
        }

        private static List<Microsoft.AspNetCore.Http.IFormFile> NormalizeImageFiles(
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
