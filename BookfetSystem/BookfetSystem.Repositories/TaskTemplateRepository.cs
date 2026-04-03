using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class TaskTemplateRepository : GenericRepository<TaskTemplate>
    {
        public TaskTemplateRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<TaskTemplate> GetAllTaskTemplateFiltered(TaskTemplate filter, int? ownerId = null)
        {
            var query = _context.TaskTemplates.AsQueryable();

            if (ownerId.HasValue)
            {
                query = query.Where(t => t.OwnerId == ownerId.Value);
            }

            if (filter.TaskTemplateId != 0)
            {
                query = query.Where(t => t.TaskTemplateId == filter.TaskTemplateId);
            }

            if (!string.IsNullOrWhiteSpace(filter.TaskName))
            {
                query = query.Where(t => t.TaskName != null && t.TaskName.ToLower().Contains(filter.TaskName.ToLower()));
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(t => t.IsActive == filter.IsActive);
            }

            return query.OrderByDescending(t => t.CreatedAt ?? System.DateTime.MinValue);
        }

        public Task<bool> HasRelatedDataAsync(int taskTemplateId)
        {
            return _context.OrderDetailStaffTasks.AnyAsync(t => t.TaskTemplateId == taskTemplateId);
        }
    }
}
