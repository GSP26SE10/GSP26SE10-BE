using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class ServiceExtraChargeCatalogRepository : GenericRepository<ServiceExtraChargeCatalog>
    {
        public ServiceExtraChargeCatalogRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<ServiceExtraChargeCatalog> GetAllFiltered(ServiceExtraChargeCatalog filter)
        {
            var query = _context.ServiceExtraChargeCatalogs
                .Include(x => x.Service)
                .Include(x => x.ExtraChargeCatalog)
                .AsQueryable();

            if (filter.ServiceExtraChargeCatalogId != 0)
            {
                query = query.Where(x => x.ServiceExtraChargeCatalogId == filter.ServiceExtraChargeCatalogId);
            }

            if (filter.ServiceId != 0)
            {
                query = query.Where(x => x.ServiceId == filter.ServiceId);
            }

            if (filter.ExtraChargeCatalogId != 0)
            {
                query = query.Where(x => x.ExtraChargeCatalogId == filter.ExtraChargeCatalogId);
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<bool> ExistsAsync(int serviceId, int extraChargeCatalogId, int? excludeId = null)
        {
            var query = _context.ServiceExtraChargeCatalogs
                .Where(x => x.ServiceId == serviceId && x.ExtraChargeCatalogId == extraChargeCatalogId);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.ServiceExtraChargeCatalogId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
