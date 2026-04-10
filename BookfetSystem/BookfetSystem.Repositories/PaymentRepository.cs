using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>
    {
        public PaymentRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Payment> GetAllPaymentFiltered(Payment filter, int? orderCustomerUserId = null)
        {
            var query = _context.Payments
                .Include(p => p.Order)
                .AsQueryable();

            if (orderCustomerUserId.HasValue)
            {
                query = query.Where(p => p.Order != null && p.Order.CustomerId == orderCustomerUserId.Value);
            }

            if (filter.PaymentId != 0)
            {
                query = query.Where(p => p.PaymentId == filter.PaymentId);
            }

            if (filter.OrderId != null)
            {
                query = query.Where(p => p.OrderId == filter.OrderId);
            }

            if (!string.IsNullOrEmpty(filter.PaymentType))
            {
                query = query.Where(p => p.PaymentType == filter.PaymentType);
            }

            if (!string.IsNullOrEmpty(filter.PaymentMethod))
            {
                query = query.Where(p => p.PaymentMethod == filter.PaymentMethod);
            }

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
            {
                query = query.Where(p => p.PaymentStatus == filter.PaymentStatus);
            }

            return query.OrderByDescending(p => p.PaidAt);
        }

        public async Task<Payment?> GetUnpaidDepositByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.OrderId == orderId &&
                    p.PaymentType != null && p.PaymentType.ToLower() == "deposit" &&
                    p.PaymentStatus != null && p.PaymentStatus.ToLower() == "unpaid");
        }

        public async Task<Payment?> GetUnpaidByOrderIdAndTypeAsync(int orderId, string paymentType)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.OrderId == orderId &&
                    p.PaymentType != null && p.PaymentType.ToLower() == paymentType.ToLower() &&
                    p.PaymentStatus != null && p.PaymentStatus.ToLower() == "unpaid");
        }

        public IQueryable<Payment> GetPaidPayments()
        {
            return _context.Payments
                .Where(p => p.PaidAt.HasValue && p.PaymentStatus != null && p.PaymentStatus.ToLower() == "paid")
                .AsQueryable();
        }
    }
}