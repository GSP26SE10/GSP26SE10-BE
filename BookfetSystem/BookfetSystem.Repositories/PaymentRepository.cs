using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>
    {
        public PaymentRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Payment> GetAllPaymentFiltered(Payment filter)
        {
            var query = _context.Payments
                .Include(p => p.Order)
                .AsQueryable();

            if (filter.PaymentId != 0)
            {
                query = query.Where(p => p.PaymentId == filter.PaymentId);
            }

            if (filter.OrderId != null)
            {
                query = query.Where(p => p.OrderId == filter.OrderId);
            }

            if (!string.IsNullOrWhiteSpace(filter.PaymentType))
            {
                query = query.Where(p => p.PaymentType != null && p.PaymentType.ToLower().Contains(filter.PaymentType.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.PaymentMethod))
            {
                query = query.Where(p => p.PaymentMethod != null && p.PaymentMethod.ToLower().Contains(filter.PaymentMethod.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
            {
                query = query.Where(p => p.PaymentStatus != null &&
                    p.PaymentStatus.ToLower().Contains(filter.PaymentStatus.ToLower()));
            }

            return query.OrderByDescending(p => p.PaidAt);
        }

    }
}