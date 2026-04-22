using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

        public Task<bool> IsMappedToAnyServiceAsync(int extraChargeCatalogId, IEnumerable<int> serviceIds)
        {
            var ids = serviceIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return Task.FromResult(false);
            }

            return _context.ServiceExtraChargeCatalogs.AnyAsync(x =>
                x.ExtraChargeCatalogId == extraChargeCatalogId &&
                ids.Contains(x.ServiceId));
        }

        public IQueryable<ExtraChargeCatalog> GetActiveByServiceId(int serviceId)
        {
            return _context.ServiceExtraChargeCatalogs
                .Where(x => x.ServiceId == serviceId)
                .Select(x => x.ExtraChargeCatalog)
                .Where(x => x.Status == "ACTIVE")
                .OrderBy(x => x.ExtraChargeCatalogId);
        }

        public IQueryable<ExtraChargeCatalog> GetActiveByOrderDetailId(int orderDetailId)
        {
            return _context.OrderServices
                .Where(os => os.OrderDetailId == orderDetailId && os.ServiceId.HasValue)
                .SelectMany(os => _context.ServiceExtraChargeCatalogs
                    .Where(sc => sc.ServiceId == os.ServiceId!.Value)
                    .Select(sc => sc.ExtraChargeCatalog))
                .Where(ec => ec.Status == "ACTIVE")
                .Distinct()
                .OrderBy(ec => ec.ExtraChargeCatalogId);
        }
    }
}
