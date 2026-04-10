using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using System;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;

namespace BookfetSystem.Services.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly GSP26SE10DBContext _dbContext;
        private readonly PaymentRepository _paymentRepository;
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentService(
            GSP26SE10DBContext dbContext,
            PaymentRepository paymentRepository,
            OrderRepository orderRepository,
            IConfiguration configuration,
            IEmailService emailService,
            IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _configuration = configuration;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PagedResponse<PaymentResponse>> GetAllPaymentFilteredAsync(PaymentFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Payment>();
            var query = _paymentRepository.GetAllPaymentFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<PaymentResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<PaymentResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResponse<PaymentResponse>> GetMyPaymentsFilteredAsync(int customerUserId, PaymentFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Payment>();
            var query = _paymentRepository.GetAllPaymentFiltered(entityFilter, customerUserId);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<PaymentResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<PaymentResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<PaymentResponse>> CreateAsync(PaymentCreateRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return new ApiResponse<PaymentResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            var entity = new Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                PaymentType = request.PaymentType.ToString(),
                PaymentMethod = request.PaymentMethod.ToString(),
                PaymentStatus = BookfetSystem.Services.Enum.PaymentStatus.UNPAID.ToString(),
                PaidAt = DateTime.UtcNow
            };

            var affected = await _paymentRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<PaymentResponse>();
                return new ApiResponse<PaymentResponse>
                {
                    Success = true,
                    Message = "Payment created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PaymentResponse>
            {
                Success = false,
                Message = "Failed to create payment.",
                Data = null
            };
        }

        public async Task<ApiResponse<PaymentResponse>> UpdateAsync(int id, PaymentUpdateRequest request)
        {
            var entity = await _paymentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<PaymentResponse>
                {
                    Success = false,
                    Message = "Payment not found.",
                    Data = null
                };
            }

            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return new ApiResponse<PaymentResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            entity.OrderId = request.OrderId;
            entity.Amount = request.Amount;
            entity.PaymentType = request.PaymentType.ToString();
            entity.PaymentMethod = request.PaymentMethod.ToString();
            if (request.PaymentStatus.HasValue)
            {
                entity.PaymentStatus = request.PaymentStatus.Value.ToString();
            }
            entity.PaidAt = request.PaidAt;

            var affected = await _paymentRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<PaymentResponse>();
                return new ApiResponse<PaymentResponse>
                {
                    Success = true,
                    Message = "Payment updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PaymentResponse>
            {
                Success = false,
                Message = "Failed to update payment.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _paymentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Payment not found.",
                    Data = false
                };
            }

            var removed = await _paymentRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Payment deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete payment.",
                Data = false
            };
        }

        public async Task<ApiResponse<object>> CreateDepositQR(int orderId, PaymentMethod paymentMethod)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            if (paymentMethod == PaymentMethod.CASH)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Cash payment is not supported for deposit. Please choose BANK_TRANSFER or ZALOPAY."
                };
            }

            if (paymentMethod == PaymentMethod.ZALOPAY)
            {
                return await CreateDepositZaloPayOrderAsync(order);
            }

            return await CreateDepositSePayQrAsync(order);
        }

        private async Task<ApiResponse<object>> CreateDepositSePayQrAsync(Order order)
        {
            var paymentCode = $"BOOKFET_{order.OrderId}";
            var qrBaseUrl = _configuration["SePay:QrBaseUrl"] ?? "https://qr.sepay.vn/img";
            var qrAccount = _configuration["SePay:QrAccountNumber"] ?? string.Empty;
            var qrBank = _configuration["SePay:QrBankCode"] ?? string.Empty;

            var existingUnpaid = await _paymentRepository.GetUnpaidDepositByOrderIdAsync(order.OrderId);
            if (existingUnpaid != null)
            {
                if (!string.Equals(existingUnpaid.PaymentMethod, PaymentMethod.BANK_TRANSFER.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"An unpaid deposit already exists with method {existingUnpaid.PaymentMethod}. Please complete or cancel that payment first."
                    };
                }

                var amt = (int)Math.Round(existingUnpaid.Amount ?? 0);
                var url = $"{qrBaseUrl}?acc={Uri.EscapeDataString(qrAccount)}&bank={Uri.EscapeDataString(qrBank)}&amount={amt}&des={Uri.EscapeDataString(paymentCode)}";

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "QR already exists for this order. Use existing payment.",
                    Data = new
                    {
                        orderId = order.OrderId,
                        paymentCode,
                        amount = existingUnpaid.Amount,
                        qrUrl = url
                    }
                };
            }

            var depositAmount = order.TotalPrice * 0.5m;

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = depositAmount,
                PaymentType = PaymentType.DEPOSIT.ToString(),
                PaymentMethod = PaymentMethod.BANK_TRANSFER.ToString(),
                PaymentStatus = PaymentStatus.UNPAID.ToString(),
            };

            await _paymentRepository.CreateAsync(payment);

            var amountInt = (int)Math.Round(depositAmount ?? 0);
            var qrUrl =
                $"{qrBaseUrl}?acc={Uri.EscapeDataString(qrAccount)}&bank={Uri.EscapeDataString(qrBank)}&amount={amountInt}&des={Uri.EscapeDataString(paymentCode)}";

            return new ApiResponse<object>
            {
                Success = true,
                Message = "QR created",
                Data = new
                {
                    orderId = order.OrderId,
                    paymentCode,
                    amount = depositAmount,
                    qrUrl
                }
            };
        }

        private async Task<ApiResponse<object>> CreateDepositZaloPayOrderAsync(Order order)
        {
            var appId = _configuration["ZaloPay:AppId"];
            var key1 = _configuration["ZaloPay:Key1"];
            var createOrderUrl = _configuration["ZaloPay:CreateOrderUrl"];
            var callbackUrl = _configuration["ZaloPay:CallbackUrl"] ?? string.Empty;
            var redirectUrl = _configuration["ZaloPay:RedirectUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(key1))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Missing ZaloPay configuration. Please set ZaloPay:AppId and ZaloPay:Key1."
                };
            }

            var existingUnpaid = await _paymentRepository.GetUnpaidDepositByOrderIdAsync(order.OrderId);
            var depositAmount = order.TotalPrice * 0.5m;
            Payment payment;

            if (existingUnpaid != null)
            {
                if (!string.Equals(existingUnpaid.PaymentMethod, PaymentMethod.ZALOPAY.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"An unpaid deposit already exists with method {existingUnpaid.PaymentMethod}. Please complete or cancel that payment first."
                    };
                }

                payment = existingUnpaid;
                if ((payment.Amount ?? 0m) <= 0m)
                {
                    payment.Amount = depositAmount;
                    await _paymentRepository.UpdateAsync(payment);
                }
            }
            else
            {
                payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = depositAmount,
                    PaymentType = PaymentType.DEPOSIT.ToString(),
                    PaymentMethod = PaymentMethod.ZALOPAY.ToString(),
                    PaymentStatus = PaymentStatus.UNPAID.ToString()
                };
                await _paymentRepository.CreateAsync(payment);
            }

            var appTransId = GenerateZaloPayAppTransId(order.OrderId, payment.PaymentId);
            var appUser = $"customer_{order.CustomerId ?? 0}";
            var amount = (int)Math.Round(payment.Amount ?? 0m);
            var appTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var embedData = JsonSerializer.Serialize(new
            {
                redirecturl = redirectUrl,
                paymentId = payment.PaymentId
            });
            var item = "[]";
            var description = $"Bookfet deposit for order #{order.OrderId}";

            var data = $"{appId}|{appTransId}|{appUser}|{amount}|{appTime}|{embedData}|{item}";
            var mac = ComputeHmacSha256(key1, data);

            var formData = new Dictionary<string, string>
            {
                ["app_id"] = appId,
                ["app_user"] = appUser,
                ["app_time"] = appTime.ToString(),
                ["amount"] = amount.ToString(),
                ["app_trans_id"] = appTransId,
                ["embed_data"] = embedData,
                ["item"] = item,
                ["description"] = description,
                ["bank_code"] = "zalopayapp",
                ["callback_url"] = callbackUrl,
                ["mac"] = mac
            };

            var client = _httpClientFactory.CreateClient();
            using var response = await client.PostAsync(createOrderUrl, new FormUrlEncodedContent(formData));
            var rawBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create ZaloPay order.",
                    Data = new
                    {
                        orderId = order.OrderId,
                        statusCode = (int)response.StatusCode,
                        response = rawBody
                    }
                };
            }

            using var json = JsonDocument.Parse(rawBody);
            var root = json.RootElement;
            var returnCode = GetJsonInt(root, "return_code", "returncode");
            if (returnCode != 1)
            {
                var subReturnCode = GetJsonInt(root, "sub_return_code", "subreturncode");
                var returnMessage = GetJsonString(root, "return_message", "returnmessage")
                    ?? "ZaloPay returned unsuccessful response.";

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Create ZaloPay order failed: {returnMessage}",
                    Data = new
                    {
                        orderId = order.OrderId,
                        paymentId = payment.PaymentId,
                        returnCode,
                        subReturnCode,
                        response = rawBody
                    }
                };
            }

            var orderUrl = GetJsonString(root, "order_url", "orderurl") ?? string.Empty;
            var zpTransToken = GetJsonString(root, "zp_trans_token", "zptranstoken") ?? string.Empty;

            return new ApiResponse<object>
            {
                Success = true,
                Message = "ZaloPay order created",
                Data = new
                {
                    orderId = order.OrderId,
                    paymentId = payment.PaymentId,
                    paymentMethod = PaymentMethod.ZALOPAY.ToString(),
                    amount = payment.Amount,
                    appTransId = appTransId,
                    orderUrl,
                    zpTransToken
                }
            };
        }

        public async Task<ApiResponse<object>> CreateFullQR(int orderId, PaymentMethod paymentMethod)
        {
            var order = await _orderRepository.GetByIdWithRelationAsync(orderId);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            if (!string.Equals(order.Status, OrderStatus.BILLING.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order must be BILLING before creating full payment QR."
                };
            }

            var remainingAmount = order.RemainingAmount ?? ((order.TotalPrice ?? 0m) - (order.DepositAmount ?? 0m));
            if (remainingAmount < 0)
            {
                remainingAmount = 0;
            }

            var extraChargeAmount = await _dbContext.OrderDetails
                .Where(x => x.OrderId == orderId)
                .SelectMany(x => x.OrderDetailExtraCharges)
                .SumAsync(x => x.TotalAmount) ?? 0m;

            var fullAmount = remainingAmount + extraChargeAmount;
            if (fullAmount <= 0)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order has no remaining amount to pay."
                };
            }

            var hasPaidFull = await _dbContext.Payments.AnyAsync(x =>
                x.OrderId == order.OrderId &&
                x.PaymentType == PaymentType.FULL.ToString() &&
                x.PaymentStatus == PaymentStatus.PAID.ToString());
            if (hasPaidFull)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order is already fully paid."
                };
            }

            if (paymentMethod == PaymentMethod.CASH)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Cash method is not supported in full QR. Use BANK_TRANSFER or ZALOPAY, or call create-full-cash endpoint."
                };
            }

            if (paymentMethod == PaymentMethod.ZALOPAY)
            {
                return await CreateFullZaloPayOrderAsync(order, remainingAmount, extraChargeAmount, fullAmount);
            }

            return await CreateFullSePayQrAsync(order, remainingAmount, extraChargeAmount, fullAmount);
        }

        private async Task<ApiResponse<object>> CreateFullSePayQrAsync(Order order, decimal remainingAmount, decimal extraChargeAmount, decimal fullAmount)
        {
            var paymentCode = $"BOOKFET_FULL_{order.OrderId}";
            var qrBaseUrl = _configuration["SePay:QrBaseUrl"] ?? "https://qr.sepay.vn/img";
            var qrAccount = _configuration["SePay:QrAccountNumber"] ?? string.Empty;
            var qrBank = _configuration["SePay:QrBankCode"] ?? string.Empty;

            var existingUnpaid = await _paymentRepository.GetUnpaidByOrderIdAndTypeAsync(order.OrderId, PaymentType.FULL.ToString());
            if (existingUnpaid != null)
            {
                if (!string.Equals(existingUnpaid.PaymentMethod, PaymentMethod.BANK_TRANSFER.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"An unpaid full payment already exists with method {existingUnpaid.PaymentMethod}. Please complete or cancel that payment first."
                    };
                }

                if ((existingUnpaid.Amount ?? 0m) != fullAmount)
                {
                    existingUnpaid.Amount = fullAmount;
                    await _paymentRepository.UpdateAsync(existingUnpaid);
                }

                var amt = (int)Math.Round(fullAmount);
                var url = $"{qrBaseUrl}?acc={Uri.EscapeDataString(qrAccount)}&bank={Uri.EscapeDataString(qrBank)}&amount={amt}&des={Uri.EscapeDataString(paymentCode)}";

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "QR already exists for this order. Use existing full payment.",
                    Data = new
                    {
                        orderId = order.OrderId,
                        paymentCode,
                        remainingAmount,
                        extraChargeAmount,
                        amount = fullAmount,
                        qrUrl = url
                    }
                };
            }

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = fullAmount,
                PaymentType = PaymentType.FULL.ToString(),
                PaymentMethod = PaymentMethod.BANK_TRANSFER.ToString(),
                PaymentStatus = PaymentStatus.UNPAID.ToString(),
            };

            await _paymentRepository.CreateAsync(payment);

            var amountInt = (int)Math.Round(fullAmount);
            var qrUrl = $"{qrBaseUrl}?acc={Uri.EscapeDataString(qrAccount)}&bank={Uri.EscapeDataString(qrBank)}&amount={amountInt}&des={Uri.EscapeDataString(paymentCode)}";

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Full QR created",
                Data = new
                {
                    orderId = order.OrderId,
                    paymentCode,
                    remainingAmount,
                    extraChargeAmount,
                    amount = fullAmount,
                    qrUrl
                }
            };
        }

        private async Task<ApiResponse<object>> CreateFullZaloPayOrderAsync(Order order, decimal remainingAmount, decimal extraChargeAmount, decimal fullAmount)
        {
            var appId = _configuration["ZaloPay:AppId"];
            var key1 = _configuration["ZaloPay:Key1"];
            var createOrderUrl = _configuration["ZaloPay:CreateOrderUrl"] ?? "https://sb-openapi.zalopay.vn/v2/create";
            var callbackUrl = _configuration["ZaloPay:CallbackUrl"] ?? string.Empty;
            var redirectUrl = _configuration["ZaloPay:RedirectUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(key1))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Missing ZaloPay configuration. Please set ZaloPay:AppId and ZaloPay:Key1."
                };
            }

            var existingUnpaid = await _paymentRepository.GetUnpaidByOrderIdAndTypeAsync(order.OrderId, PaymentType.FULL.ToString());
            Payment payment;

            if (existingUnpaid != null)
            {
                if (!string.Equals(existingUnpaid.PaymentMethod, PaymentMethod.ZALOPAY.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"An unpaid full payment already exists with method {existingUnpaid.PaymentMethod}. Please complete or cancel that payment first."
                    };
                }

                payment = existingUnpaid;
                if ((payment.Amount ?? 0m) != fullAmount)
                {
                    payment.Amount = fullAmount;
                    await _paymentRepository.UpdateAsync(payment);
                }
            }
            else
            {
                payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = fullAmount,
                    PaymentType = PaymentType.FULL.ToString(),
                    PaymentMethod = PaymentMethod.ZALOPAY.ToString(),
                    PaymentStatus = PaymentStatus.UNPAID.ToString()
                };
                await _paymentRepository.CreateAsync(payment);
            }

            var appTransId = GenerateZaloPayAppTransId(order.OrderId, payment.PaymentId);
            var appUser = $"customer_{order.CustomerId ?? 0}";
            var amount = (int)Math.Round(payment.Amount ?? 0m);
            var appTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var embedData = JsonSerializer.Serialize(new
            {
                redirecturl = redirectUrl,
                paymentId = payment.PaymentId
            });
            var item = "[]";
            var description = $"Bookfet full payment for order #{order.OrderId}";

            var data = $"{appId}|{appTransId}|{appUser}|{amount}|{appTime}|{embedData}|{item}";
            var mac = ComputeHmacSha256(key1, data);

            var formData = new Dictionary<string, string>
            {
                ["app_id"] = appId,
                ["app_user"] = appUser,
                ["app_time"] = appTime.ToString(),
                ["amount"] = amount.ToString(),
                ["app_trans_id"] = appTransId,
                ["embed_data"] = embedData,
                ["item"] = item,
                ["description"] = description,
                ["bank_code"] = "zalopayapp",
                ["callback_url"] = callbackUrl,
                ["mac"] = mac
            };

            var client = _httpClientFactory.CreateClient();
            using var response = await client.PostAsync(createOrderUrl, new FormUrlEncodedContent(formData));
            var rawBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create full ZaloPay order.",
                    Data = new
                    {
                        orderId = order.OrderId,
                        statusCode = (int)response.StatusCode,
                        response = rawBody
                    }
                };
            }

            using var json = JsonDocument.Parse(rawBody);
            var root = json.RootElement;
            var returnCode = GetJsonInt(root, "return_code", "returncode");
            if (returnCode != 1)
            {
                var subReturnCode = GetJsonInt(root, "sub_return_code", "subreturncode");
                var returnMessage = GetJsonString(root, "return_message", "returnmessage")
                    ?? "ZaloPay returned unsuccessful response.";

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Create full ZaloPay order failed: {returnMessage}",
                    Data = new
                    {
                        orderId = order.OrderId,
                        paymentId = payment.PaymentId,
                        returnCode,
                        subReturnCode,
                        response = rawBody
                    }
                };
            }

            var orderUrl = GetJsonString(root, "order_url", "orderurl") ?? string.Empty;
            var zpTransToken = GetJsonString(root, "zp_trans_token", "zptranstoken") ?? string.Empty;

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Full ZaloPay order created",
                Data = new
                {
                    orderId = order.OrderId,
                    paymentId = payment.PaymentId,
                    paymentMethod = PaymentMethod.ZALOPAY.ToString(),
                    remainingAmount,
                    extraChargeAmount,
                    amount = payment.Amount,
                    appTransId = appTransId,
                    orderUrl,
                    zpTransToken
                }
            };
        }

        public async Task<ApiResponse<object>> CreateFullCashPayment(int orderId)
        {
            var order = await _orderRepository.GetByIdWithRelationAsync(orderId);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            if (!string.Equals(order.Status, OrderStatus.BILLING.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order must be BILLING before creating full cash payment."
                };
            }

            var hasPaidFull = await _dbContext.Payments.AnyAsync(x =>
                x.OrderId == order.OrderId &&
                x.PaymentType == PaymentType.FULL.ToString() &&
                x.PaymentStatus == PaymentStatus.PAID.ToString());
            if (hasPaidFull)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order is already fully paid."
                };
            }

            var remainingAmount = order.RemainingAmount ?? ((order.TotalPrice ?? 0m) - (order.DepositAmount ?? 0m));
            if (remainingAmount < 0)
            {
                remainingAmount = 0;
            }

            var extraChargeAmount = await _dbContext.OrderDetails
                .Where(x => x.OrderId == orderId)
                .SelectMany(x => x.OrderDetailExtraCharges)
                .SumAsync(x => x.TotalAmount) ?? 0m;

            var fullAmount = remainingAmount + extraChargeAmount;
            if (fullAmount <= 0)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order has no remaining amount to pay."
                };
            }

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = fullAmount,
                PaymentType = PaymentType.FULL.ToString(),
                PaymentMethod = PaymentMethod.CASH.ToString(),
                PaymentStatus = PaymentStatus.PAID.ToString(),
                PaidAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment);

            order.RemainingAmount = 0;
            order.Status = OrderStatus.COMPLETED.ToString();
            await _orderRepository.UpdateAsync(order);
            await SendOrderCompletedEmailAsync(order, "tiền mặt");

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Full cash payment created successfully.",
                Data = new
                {
                    orderId = order.OrderId,
                    paymentId = payment.PaymentId,
                    remainingAmount,
                    extraChargeAmount,
                    amount = fullAmount,
                    paymentType = payment.PaymentType,
                    paymentMethod = payment.PaymentMethod,
                    paymentStatus = payment.PaymentStatus,
                    paidAt = payment.PaidAt
                }
            };
        }

        public async Task<ApiResponse<object>> RefundRejectedOrderDepositAsync(int orderId, string? reason)
        {
            var order = await _orderRepository.GetByIdWithRelationAsync(orderId);
            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found."
                };
            }

            var paidZaloPayDeposit = GetLatestPaidZaloPayDeposit(order);
            if (paidZaloPayDeposit == null)
            {
                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "No paid ZaloPay deposit to refund."
                };
            }

            var fullDepositAmount = paidZaloPayDeposit.Amount ?? 0m;
            if (fullDepositAmount <= 0m)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid deposit amount for refund."
                };
            }

            return await RefundOrderDepositInternalAsync(order, paidZaloPayDeposit, fullDepositAmount, reason);
        }

        public async Task<ApiResponse<object>> RefundOrderDepositByAmountAsync(int orderId, decimal refundAmount, string? reason)
        {
            var order = await _orderRepository.GetByIdWithRelationAsync(orderId);
            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found."
                };
            }

            if (refundAmount <= 0m)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Refund amount must be greater than 0."
                };
            }

            var paidZaloPayDeposit = GetLatestPaidZaloPayDeposit(order);
            if (paidZaloPayDeposit == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "No paid ZaloPay deposit to refund."
                };
            }

            var paidAmount = paidZaloPayDeposit.Amount ?? 0m;
            if (refundAmount > paidAmount)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Refund amount exceeds paid deposit. Max refundable: {paidAmount:0.##}."
                };
            }

            return await RefundOrderDepositInternalAsync(order, paidZaloPayDeposit, refundAmount, reason);
        }

        private async Task<ApiResponse<object>> RefundOrderDepositInternalAsync(Order order, Payment paidZaloPayDeposit, decimal refundAmount, string? reason)
        {
            var metadata = DeserializeZaloPayMetadata(order.MtdZlp);
            var paymentMetadata = metadata.Payments.FirstOrDefault(x => x.PaymentId == paidZaloPayDeposit.PaymentId);
            var zpTransId = paymentMetadata?.ZpTransId;
            if (string.IsNullOrWhiteSpace(zpTransId))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Missing zp_trans_id for paid ZaloPay deposit. Cannot process refund automatically."
                };
            }

            if (paymentMetadata?.Refunds?.Any() == true)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "A refund request already exists for this deposit payment."
                };
            }

            var appId = _configuration["ZaloPay:AppId"];
            var key1 = _configuration["ZaloPay:Key1"];
            var refundUrl = _configuration["ZaloPay:RefundUrl"] ?? "https://sb-openapi.zalopay.vn/v2/refund";
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(key1))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Missing ZaloPay config for refund (AppId/Key1)."
                };
            }

            var amount = (long)Math.Round(refundAmount);
            if (amount <= 0)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid refund amount."
                };
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var mRefundId = $"{GetVietnamNow():yyMMdd}_{appId}_{order.OrderId}_{paidZaloPayDeposit.PaymentId}_{timestamp}";
            var description = string.IsNullOrWhiteSpace(reason)
                ? $"Refund deposit for order #{order.OrderId}"
                : $"Refund deposit for order #{order.OrderId} - {reason.Trim()}";
            if (description.Length > 100)
            {
                description = description.Substring(0, 100);
            }

            var macInput = $"{appId}|{zpTransId}|{amount}|{description}|{timestamp}";
            var mac = ComputeHmacSha256(key1, macInput);

            var formData = new Dictionary<string, string>
            {
                ["app_id"] = appId,
                ["m_refund_id"] = mRefundId,
                ["zp_trans_id"] = zpTransId,
                ["amount"] = amount.ToString(),
                ["timestamp"] = timestamp.ToString(),
                ["description"] = description,
                ["mac"] = mac
            };

            var client = _httpClientFactory.CreateClient();
            using var response = await client.PostAsync(refundUrl, new FormUrlEncodedContent(formData));
            var rawBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to call ZaloPay refund API.",
                    Data = new
                    {
                        orderId = order.OrderId,
                        paymentId = paidZaloPayDeposit.PaymentId,
                        statusCode = (int)response.StatusCode,
                        response = rawBody
                    }
                };
            }

            using var json = JsonDocument.Parse(rawBody);
            var root = json.RootElement;
            var returnCode = GetJsonInt(root, "return_code", "returncode");
            var subReturnCode = GetJsonInt(root, "sub_return_code", "subreturncode");
            var returnMessage = GetJsonString(root, "return_message", "returnmessage") ?? string.Empty;

            var isRefundAccepted = returnCode == 1 || returnCode == 3;
            if (!isRefundAccepted)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"ZaloPay refund failed: {returnMessage}",
                    Data = new
                    {
                        orderId = order.OrderId,
                        paymentId = paidZaloPayDeposit.PaymentId,
                        returnCode,
                        subReturnCode,
                        response = rawBody
                    }
                };
            }

            paymentMetadata ??= new ZaloPayPaymentMetadata
            {
                PaymentId = paidZaloPayDeposit.PaymentId
            };
            paymentMetadata.Refunds ??= new List<ZaloPayRefundMetadata>();
            paymentMetadata.Refunds.Add(new ZaloPayRefundMetadata
            {
                MRefundId = mRefundId,
                ReturnCode = returnCode,
                SubReturnCode = subReturnCode,
                Amount = amount,
                CreatedAt = DateTime.UtcNow
            });

            if (!metadata.Payments.Any(x => x.PaymentId == paymentMetadata.PaymentId))
            {
                metadata.Payments.Add(paymentMetadata);
            }

            order.MtdZlp = JsonSerializer.Serialize(metadata);
            await _orderRepository.UpdateAsync(order);

            return new ApiResponse<object>
            {
                Success = true,
                Message = returnCode == 1 ? "ZaloPay refund completed." : "ZaloPay refund is processing.",
                Data = new
                {
                    orderId = order.OrderId,
                    paymentId = paidZaloPayDeposit.PaymentId,
                    amount,
                    mRefundId,
                    returnCode,
                    subReturnCode
                }
            };
        }

        private static Payment? GetLatestPaidZaloPayDeposit(Order order)
        {
            return (order.Payments ?? new List<Payment>())
                .Where(p =>
                    string.Equals(p.PaymentType, PaymentType.DEPOSIT.ToString(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.PaymentMethod, PaymentMethod.ZALOPAY.ToString(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.PaymentStatus, PaymentStatus.PAID.ToString(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.PaidAt ?? DateTime.MinValue)
                .FirstOrDefault();
        }

        private async Task SendOrderCompletedEmailAsync(Order order, string paymentMethodLabel)
        {
            var toEmail = order.Customer?.Email;
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            var customerName = string.IsNullOrWhiteSpace(order.Customer?.FullName)
                ? "Quý khách"
                : order.Customer!.FullName;
            var partyCards = (order.OrderDetails ?? new List<OrderDetail>())
                .OrderBy(x => x.StartTime)
                .Select(detail =>
                {
                    var startTimeText = detail.StartTime.HasValue
                        ? ToVietnamTime(detail.StartTime.Value).ToString("dd/MM/yyyy HH:mm")
                        : "Chưa xác định";
                    var menuName = string.IsNullOrWhiteSpace(detail.Menu?.MenuName) ? "Tiệc đơn giản" : detail.Menu!.MenuName;
                    var menuImageUrl = GetFirstImageUrl(detail.Menu?.ImgUrl);
                    var imageHtml = string.IsNullOrWhiteSpace(menuImageUrl)
                        ? @"<div style=""height:120px;border-radius:10px;background:#e2e8f0;color:#334155;display:flex;align-items:center;justify-content:center;font-weight:600;"">Tiệc đơn giản</div>"
                        : $@"<img src=""{menuImageUrl}"" alt=""{menuName}"" style=""width:100%;height:120px;object-fit:cover;border-radius:10px;display:block;"" />";

                    return $@"
<div style=""border:1px solid #e2e8f0;border-radius:10px;padding:12px;margin-bottom:10px;background:#f8fafc;"">
  {imageHtml}
  <p style=""margin:10px 0 4px 0;font-weight:700;"">{menuName}</p>
  <p style=""margin:0;color:#334155;"">Mã tiệc: <strong>#{detail.OrderDetailId}</strong></p>
  <p style=""margin:4px 0 0 0;color:#334155;"">Thời gian tổ chức: <strong>{startTimeText}</strong></p>
</div>";
                })
                .ToList();
            var partySection = partyCards.Count == 0
                ? string.Empty
                : $@"
<div style=""margin:14px 0;"">
  <p style=""margin:0 0 8px 0;font-weight:700;"">Các tiệc đã được tổ chức và hoàn thành:</p>
  {string.Join(string.Empty, partyCards)}
</div>";
            var partyPlainText = string.Join(
                "; ",
                (order.OrderDetails ?? new List<OrderDetail>())
                    .OrderBy(x => x.StartTime)
                    .Select(detail =>
                    {
                        var startTimeText = detail.StartTime.HasValue
                            ? ToVietnamTime(detail.StartTime.Value).ToString("dd/MM/yyyy HH:mm")
                            : "Chua xac dinh";
                        return $"tiec #{detail.OrderDetailId} luc {startTimeText}";
                    }));

            var subject = $"[Bookfet] Đơn hàng #{order.OrderId} đã hoàn thành";
            var htmlBody = $@"
<div style=""font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:16px;background:#f8fafc;color:#0f172a;"">
  <div style=""background:#ffffff;border-radius:12px;padding:20px;border:1px solid #e2e8f0;"">
    <h2 style=""margin:0 0 12px 0;"">Đơn hàng đã hoàn thành</h2>
    <p style=""margin:0 0 10px 0;"">Xin chào <strong>{customerName}</strong>,</p>
    <p style=""margin:0 0 12px 0;"">Bookfet trân trọng thông báo đơn hàng <strong>#{order.OrderId}</strong> của bạn đã hoàn tất thanh toán qua <strong>{paymentMethodLabel}</strong> và được ghi nhận hoàn thành toàn bộ dịch vụ.</p>
    <p style=""margin:0 0 14px 0;"">
      Trạng thái:
      <span style=""display:inline-block;padding:4px 10px;border-radius:999px;background:#dcfce7;color:#16a34a;font-weight:700;"">
        HOÀN THÀNH
      </span>
    </p>
    {partySection}
    <p style=""margin:0 0 10px 0;"">Đội ngũ vận hành đã kết thúc các hạng mục của tiệc theo kế hoạch, đồng thời hệ thống đã cập nhật dữ liệu thanh toán và trạng thái đơn hàng của bạn.</p>
    <p style=""margin:0 0 10px 0;"">Nếu bạn cần hỗ trợ thêm về hóa đơn, lịch sử đơn hàng hoặc muốn đặt tiệc mới, vui lòng liên hệ Bookfet qua kênh chat hoặc hotline để được phục vụ nhanh nhất.</p>
    <p style=""margin:0;"">Bookfet cảm ơn bạn đã đồng hành và rất mong được tiếp tục phục vụ trong các sự kiện sắp tới.</p>
  </div>
</div>";

            var plainText =
                $"Xin chao {customerName}. Bookfet tran trong thong bao don hang #{order.OrderId} da hoan tat thanh toan qua {paymentMethodLabel} va chuyen sang trang thai HOAN THANH. Cac tiec da duoc to chuc: {partyPlainText}. Neu ban can ho tro them, vui long lien he Bookfet qua kenh chat hoac hotline.";

            try
            {
                await _emailService.SendAsync(toEmail, subject, htmlBody, plainText);
            }
            catch
            {
                // Email failure must not break payment completion flow.
            }
        }

        private static DateTime ToVietnamTime(DateTime input)
        {
            var utc = input.Kind switch
            {
                DateTimeKind.Utc => input,
                DateTimeKind.Local => input.ToUniversalTime(),
                _ => DateTime.SpecifyKind(input, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utc, GetVietnamTimeZone());
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }

        private static string? GetFirstImageUrl(string? rawImgUrl)
        {
            if (string.IsNullOrWhiteSpace(rawImgUrl))
            {
                return null;
            }

            var trimmed = rawImgUrl.Trim();
            try
            {
                if (trimmed.StartsWith("["))
                {
                    var images = JsonSerializer.Deserialize<List<string>>(trimmed);
                    return images?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                }

                if (trimmed.StartsWith("\""))
                {
                    var single = JsonSerializer.Deserialize<string>(trimmed);
                    return string.IsNullOrWhiteSpace(single) ? null : single;
                }
            }
            catch
            {
                // Fallback below when image JSON is malformed.
            }

            return trimmed;
        }

        private static string ComputeHmacSha256(string key, string input)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private static ZaloPayMetadata DeserializeZaloPayMetadata(string? rawMetadata)
        {
            if (string.IsNullOrWhiteSpace(rawMetadata))
            {
                return new ZaloPayMetadata();
            }

            try
            {
                return JsonSerializer.Deserialize<ZaloPayMetadata>(rawMetadata) ?? new ZaloPayMetadata();
            }
            catch
            {
                return new ZaloPayMetadata();
            }
        }

        private static DateTime GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetVietnamTimeZone());
        }

        private static string GenerateZaloPayAppTransId(int orderId, int paymentId)
        {
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetVietnamTimeZone());
            var uniqueSuffix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            // ZaloPay requires yyMMdd prefix in GMT+7 and app_trans_id must be unique.
            return $"{vietnamNow:yyMMdd}_{orderId}_{paymentId}_{uniqueSuffix}";
        }

        private static int GetJsonInt(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (!root.TryGetProperty(name, out var value))
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var intValue))
                {
                    return intValue;
                }

                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out intValue))
                {
                    return intValue;
                }
            }

            return -1;
        }

        private static string? GetJsonString(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (!root.TryGetProperty(name, out var value))
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }

                if (value.ValueKind == JsonValueKind.Number || value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
                {
                    return value.ToString();
                }
            }

            return null;
        }

        private sealed class ZaloPayMetadata
        {
            public List<ZaloPayPaymentMetadata> Payments { get; set; } = new();
        }

        private sealed class ZaloPayPaymentMetadata
        {
            public int PaymentId { get; set; }
            public string? AppTransId { get; set; }
            public string? ZpTransId { get; set; }
            public List<ZaloPayRefundMetadata>? Refunds { get; set; }
        }

        private sealed class ZaloPayRefundMetadata
        {
            public string? MRefundId { get; set; }
            public int ReturnCode { get; set; }
            public int SubReturnCode { get; set; }
            public long Amount { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
