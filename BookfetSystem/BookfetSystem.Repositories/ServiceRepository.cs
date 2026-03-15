using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class ServiceRepository : GenericRepository<Service>
    {
        public ServiceRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Service> GetAllServiceFiltered(Service filter)
        {
            var query = _context.Services
                .Include(x => x.FeedbackServices)
                .Include(x => x.OrderServices)
                .AsQueryable();

            if (filter.ServiceId != 0)
            {
                query = query.Where(x => x.ServiceId == filter.ServiceId);
            }

            if (!string.IsNullOrWhiteSpace(filter.ServiceName))
            {
                query = query.Where(x =>
                    x.ServiceName.ToLower().Contains(filter.ServiceName.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(x => x.Status == filter.Status);
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<Service?> GetByIdWithRelationAsync(int id)
        {
            return await _context.Services
                .Include(x => x.FeedbackServices)
                .Include(x => x.OrderServices)
                .FirstOrDefaultAsync(x => x.ServiceId == id);
        }

        public Task<bool> HasRelatedDataAsync(int serviceId)
        {
            return _context.OrderServices
                .AnyAsync(x => x.ServiceId == serviceId);
        }
    }
}