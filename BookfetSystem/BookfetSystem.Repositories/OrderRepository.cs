using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class OrderRepository : GenericRepository<Order>
    {
        public OrderRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Order> GetAllOrderFiltered(Order filter)
        {
            var query = _context.Orders
                .Include(x => x.Customer)
                .Include(x => x.OrderDetails)
                    .ThenInclude(od => od.Menu)
                .Include(x => x.OrderDetails)
                    .ThenInclude(od => od.PartyCategory)
                .Include(x => x.Payments)
                .AsQueryable();

            if (filter.OrderId != 0)
            {
                query = query.Where(x => x.OrderId == filter.OrderId);
            }

            if (filter.CustomerId.HasValue && filter.CustomerId.Value != 0)
            {
                query = query.Where(x => x.CustomerId == filter.CustomerId);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(x => x.Status == filter.Status);
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<Order?> GetByIdWithRelationAsync(int id)
        {
            return await _context.Orders
                .Include(x => x.Customer)
                .Include(x => x.OrderDetails)
                    .ThenInclude(od => od.Menu)
                .Include(x => x.OrderDetails)
                    .ThenInclude(od => od.PartyCategory)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.OrderId == id);
        }

        public async Task<bool> HasRelatedDataAsync(int orderId)
        {
            var hasOrderDetails = await _context.OrderDetails.AnyAsync(x => x.OrderId == orderId);
            var hasPayments = await _context.Payments.AnyAsync(x => x.OrderId == orderId);

            return hasOrderDetails || hasPayments;
        }

        public IQueryable<Order> GetDepositedApprovedOrdersForAssignment()
        {
            return _context.Orders
                .Include(x => x.Customer)
                .Include(x => x.OrderDetails)
                    .ThenInclude(od => od.Menu)
                .Include(x => x.OrderDetails)
                    .ThenInclude(od => od.PartyCategory)
                .Include(x => x.Payments)
                .Where(x => x.Status == "PENDING")  
                .Where(x => (x.DepositAmount ?? 0) > 0 ||
                            x.Payments.Any(p => p.PaymentStatus == "PAID"))
                .Where(x => x.OrderDetails.Any(od => !od.StaffGroupId.HasValue))
                .OrderByDescending(x => x.CreatedAt)
                .AsQueryable();
        }
    }
}