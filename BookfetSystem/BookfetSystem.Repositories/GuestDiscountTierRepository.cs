using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class GuestDiscountTierRepository : GenericRepository<GuestDiscountTier>
    {
        public GuestDiscountTierRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<GuestDiscountTier> GetAllFiltered(GuestDiscountTier filter)
        {
            var query = _context.GuestDiscountTiers.AsQueryable();

            if (filter.GuestDiscountTierId != 0)
            {
                query = query.Where(x => x.GuestDiscountTierId == filter.GuestDiscountTierId);
            }

            if (filter.MinGuestCount != 0)
            {
                query = query.Where(x => x.MinGuestCount == filter.MinGuestCount);
            }

            if (filter.DiscountPercent != 0)
            {
                query = query.Where(x => x.DiscountPercent == filter.DiscountPercent);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(x => x.Status != null && x.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            return query.OrderBy(x => x.MinGuestCount);
        }
    }
}
