using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class OrderDetailStaffTaskRepository : GenericRepository<OrderDetailStaffTask>
    {
        public OrderDetailStaffTaskRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<OrderDetailStaffTask> GetAllOrderDetailStaffTaskFiltered(OrderDetailStaffTask filter)
        {
            var query = _context.OrderDetailStaffTasks
                .Include(t => t.OrderDetail)
                .Include(t => t.Staff)
                .AsQueryable();

            if (filter.TaskId != 0)
            {
                query = query.Where(t => t.TaskId == filter.TaskId);
            }

            if (filter.OrderDetailId != null)
            {
                query = query.Where(t => t.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.StaffId != null)
            {
                query = query.Where(t => t.StaffId == filter.StaffId);
            }

            if (!string.IsNullOrEmpty(filter.TaskStatus))
            {
                query = query.Where(t => t.TaskStatus != null && t.TaskStatus.Contains(filter.TaskStatus));
            }

            if (!string.IsNullOrEmpty(filter.TaskName))
            {
                query = query.Where(t => t.TaskName != null && t.TaskName.Contains(filter.TaskName));
            }

            return query.OrderByDescending(t => t.StartTime);
        }

        public IQueryable<OrderDetailStaffTask> GetMyTasksByStaffId(int staffId)
        {
            return _context.OrderDetailStaffTasks
                .Include(t => t.OrderDetail)
                    .ThenInclude(od => od.Menu)
                .Include(t => t.OrderDetail)
                    .ThenInclude(od => od.PartyCategory)
                .Where(t => t.StaffId == staffId)
                .OrderByDescending(t => t.StartTime);
        }
    }
}
