using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class ExtraChargeCatalogRepository : GenericRepository<ExtraChargeCatalog>
    {
        public ExtraChargeCatalogRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<ExtraChargeCatalog> GetAllFiltered(ExtraChargeCatalog filter, bool activeOnly = false)
        {
            var query = _context.ExtraChargeCatalogs.AsQueryable();

            if (filter.ExtraChargeCatalogId != 0)
            {
                query = query.Where(x => x.ExtraChargeCatalogId == filter.ExtraChargeCatalogId);
            }

            if (!string.IsNullOrWhiteSpace(filter.ChargeType))
            {
                query = query.Where(x => x.ChargeType != null && x.ChargeType.ToLower().Contains(filter.ChargeType.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                query = query.Where(x => x.Title != null && x.Title.ToLower().Contains(filter.Title.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(x => x.Status != null && x.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            if (activeOnly)
            {
                query = query.Where(x => x.Status == "ACTIVE");
            }

            return query.OrderBy(x => x.ExtraChargeCatalogId);
        }

        public Task<bool> HasRelatedDataAsync(int extraChargeCatalogId)
        {
            return _context.OrderDetailExtraCharges.AnyAsync(x => x.ExtraChargeCatalogId == extraChargeCatalogId);
        }
    }
}
