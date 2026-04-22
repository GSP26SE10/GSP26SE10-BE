using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class OrderDetailRepository : GenericRepository<OrderDetail>
    {
        public OrderDetailRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<OrderDetail> GetAllOrderDetailFiltered(OrderDetail filter)
        {
            var query = _context.OrderDetails
                .Include(x => x.Menu)
                .Include(x => x.PartyCategory)
                .Include(x => x.StaffGroup)
                .Include(x => x.Order)
                .AsQueryable();

            if (filter.OrderDetailId != 0)
            {
                query = query.Where(x => x.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.OrderId.HasValue && filter.OrderId.Value != 0)
            {
                query = query.Where(x => x.OrderId == filter.OrderId);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(x => x.Status == filter.Status);
            }

            if (!string.IsNullOrEmpty(filter.Type))
            {
                query = query.Where(x => x.Type == filter.Type);
            }

            if (filter.MenuId.HasValue && filter.MenuId.Value != 0)
            {
                query = query.Where(x => x.MenuId == filter.MenuId);
            }

            if (filter.PartyCategoryId.HasValue && filter.PartyCategoryId.Value != 0)
            {
                query = query.Where(x => x.PartyCategoryId == filter.PartyCategoryId);
            }

            return query.OrderByDescending(x => x.StartTime);
        }

        public async Task<OrderDetail?> GetByIdWithRelationAsync(int id)
        {
            return await _context.OrderDetails
                .Include(x => x.Menu)
                .Include(x => x.PartyCategory)
                .Include(x => x.StaffGroup)
                .Include(x => x.Order)
                .Include(x => x.OrderServices)
                .FirstOrDefaultAsync(x => x.OrderDetailId == id);
        }

        public async Task<int> AssignOrderToStaffGroupAsync(int orderId, int staffGroupId, string preparingStatus)
        {
            var orderDetails = await _context.OrderDetails
                .Where(x => x.OrderId == orderId && !x.StaffGroupId.HasValue)
                .ToListAsync();

            foreach (var orderDetail in orderDetails)
            {
                orderDetail.StaffGroupId = staffGroupId;
                // Repository layer should not depend on Services enums.
                if (string.IsNullOrWhiteSpace(orderDetail.Status) ||
                    orderDetail.Status == "PENDING" ||
                    orderDetail.Status == "APPROVED")
                {
                    orderDetail.Status = preparingStatus;
                }
            }

            return await _context.SaveChangesAsync();
        }
    }
}