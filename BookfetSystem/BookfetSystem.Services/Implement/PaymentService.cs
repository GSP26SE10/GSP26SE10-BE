using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.EntityFrameworkCore;
using static BookfetSystem.Services.Models.Request.PaymentRequest;

namespace BookfetSystem.Services.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly GenericRepository<Payment> _paymentRepository;
        private readonly GSP26SE10DBContext _context;

        public PaymentService(GSP26SE10DBContext context)
        {
            _context = context;
            _paymentRepository = new GenericRepository<Payment>(context);
        }

        public async Task<object> GetAllFilteredAsync(PaymentFilterRequest filter, int page, int pageSize)
        {
            var query = _context.Payments.AsQueryable();

            if (filter.OrderId.HasValue)
            {
                query = query.Where(x => x.OrderId == filter.OrderId);
            }

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
            {
                query = query.Where(x => x.PaymentStatus == filter.PaymentStatus);
            }

            if (!string.IsNullOrEmpty(filter.PaymentMethod))
            {
                query = query.Where(x => x.PaymentMethod == filter.PaymentMethod);
            }

            var total = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(PaymentCreateRequest request)
        {
            var payment = new Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                PaymentType = request.PaymentType,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = request.PaymentStatus,
                PaidAt = request.PaidAt
            };

            await _paymentRepository.CreateAsync(payment);

            return (true, "Payment created successfully");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, PaymentUpdateRequest request)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);

            if (payment == null)
            {
                return (false, "Payment not found");
            }

            payment.OrderId = request.OrderId;
            payment.Amount = request.Amount;
            payment.PaymentType = request.PaymentType;
            payment.PaymentMethod = request.PaymentMethod;
            payment.PaymentStatus = request.PaymentStatus;
            payment.PaidAt = request.PaidAt;

            await _paymentRepository.UpdateAsync(payment);

            return (true, "Payment updated successfully");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);

            if (payment == null)
            {
                return (false, "Payment not found");
            }

            await _paymentRepository.RemoveAsync(payment);

            return (true, "Payment deleted successfully");
        }
    }
}