using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class OrderDetailExtraChargeRepository : GenericRepository<OrderDetailExtraCharge>
    {
        public OrderDetailExtraChargeRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<OrderDetailExtraCharge> GetAllFiltered(OrderDetailExtraCharge filter)
        {
            var query = _context.OrderDetailExtraCharges
                .Include(x => x.OrderDetail)
                .Include(x => x.ExtraChargeCatalog)
                .Include(x => x.CreateByNavigation)
                .AsQueryable();

            if (filter.OrderDetailExtraChargeId != 0)
            {
                query = query.Where(x => x.OrderDetailExtraChargeId == filter.OrderDetailExtraChargeId);
            }

            if (filter.OrderDetailId.HasValue)
            {
                query = query.Where(x => x.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.ExtraChargeCatalogId.HasValue)
            {
                query = query.Where(x => x.ExtraChargeCatalogId == filter.ExtraChargeCatalogId);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(x => x.Status != null && x.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
}
