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

        public IQueryable<Service> GetAllServiceFiltered(Service filter, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Services.AsQueryable();

            if (filter.ServiceId != 0)
            {
                query = query.Where(x => x.ServiceId == filter.ServiceId);
            }

            if (!string.IsNullOrWhiteSpace(filter.ServiceName))
            {
                query = query.Where(x => x.ServiceName.ToLower().Contains(filter.ServiceName.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(x => x.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(x => x.BasePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => x.BasePrice <= maxPrice.Value);
            }

            return query.OrderBy(x => x.ServiceName);
        }

        public async Task<bool> HasRelatedDataAsync(int serviceId)
        {
            return await _context.OrderServices.AnyAsync(x => x.ServiceId == serviceId);
        }
    }
}