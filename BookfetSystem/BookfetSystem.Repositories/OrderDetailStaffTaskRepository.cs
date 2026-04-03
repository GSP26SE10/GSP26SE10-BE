using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class OrderDetailStaffTaskRepository : GenericRepository<OrderDetailStaffTask>
    {
        public OrderDetailStaffTaskRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<OrderDetailStaffTask> GetAllOrderDetailStaffTaskFiltered(OrderDetailStaffTask filter, string? taskName)
        {
            var query = _context.OrderDetailStaffTasks
                .Include(t => t.OrderDetail)
                .Include(t => t.Staff)
            .Include(t => t.TaskTemplate)
                .AsQueryable();

            if (filter.TaskId != 0)
            {
                query = query.Where(t => t.TaskId == filter.TaskId);
            }

            if (filter.OrderDetailId != null)
            {
                query = query.Where(t => t.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.TaskTemplateId != 0)
            {
                query = query.Where(t => t.TaskTemplateId == filter.TaskTemplateId);
            }

            if (filter.StaffId != null)
            {
                query = query.Where(t => t.StaffId == filter.StaffId);
            }

            if (!string.IsNullOrEmpty(filter.TaskStatus))
            {
                query = query.Where(t => t.TaskStatus == filter.TaskStatus);
            }

            if (!string.IsNullOrEmpty(taskName))
            {
                query = query.Where(t => t.TaskTemplate != null && t.TaskTemplate.TaskName != null && t.TaskTemplate.TaskName.Contains(taskName));
            }

            return query.OrderByDescending(t => t.StartTime);
        }

        public IQueryable<OrderDetailStaffTask> GetMyTasksByStaffId(int staffId)
        {
            return _context.OrderDetailStaffTasks
                .Include(t => t.TaskTemplate)
                .Include(t => t.OrderDetail)
                    .ThenInclude(od => od.Menu)
                .Include(t => t.OrderDetail)
                    .ThenInclude(od => od.PartyCategory)
                .Include(t => t.OrderDetail)
                    .ThenInclude(od => od.Order)
                .Where(t => t.StaffId == staffId)
                .OrderByDescending(t => t.StartTime);
        }

        public IQueryable<OrderDetailStaffTask> GetOverdueTaskCandidates(DateTime utcNow)
        {
            return _context.OrderDetailStaffTasks
                .Include(t => t.OrderDetail)
                .Include(t => t.Staff)
                .Include(t => t.TaskTemplate)
                .Where(t =>
                    t.EndTime.HasValue &&
                    t.EndTime.Value < utcNow &&
                    t.TaskStatus != "COMPLETED" &&
                    t.TaskStatus != "CANCELLED" &&
                    t.TaskStatus != "OVERDUE");
        }
    }
}
