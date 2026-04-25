using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class PartyCategoryRepository : GenericRepository<PartyCategory>
    {
        public PartyCategoryRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<PartyCategory> GetAllPartyCategoryFiltered(PartyCategory filter)
        {
            var query = _context.PartyCategories
                .Include(pc => pc.PartyCategoryMenus)
                .Include(pc => pc.OrderDetails)
                .AsQueryable();

            if (filter.PartyCategoryId != 0)
            {
                query = query.Where(pc => pc.PartyCategoryId == filter.PartyCategoryId);
            }

            if (!string.IsNullOrWhiteSpace(filter.PartyCategoryName))
            {
                query = query.Where(pc => pc.PartyCategoryName.ToLower()
                    .Contains(filter.PartyCategoryName.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(pc => pc.Status == filter.Status);
            }

            if (filter.NumberOfGuests.HasValue)
            {
                query = query.Where(pc => pc.NumberOfGuests == filter.NumberOfGuests);
            }

            if (filter.ServiceDurationMinutes.HasValue)
            {
                query = query.Where(pc => pc.ServiceDurationMinutes == filter.ServiceDurationMinutes);
            }

            return query.OrderBy(pc => pc.PartyCategoryName);
        }

        public async Task<bool> HasRelatedDataAsync(int partyCategoryId)
        {
            var hasOrderDetail = await _context.OrderDetails
                .AnyAsync(od => od.PartyCategoryId == partyCategoryId);

            if (hasOrderDetail)
            {
                return true;
            }

            return await _context.PartyCategoryMenus
                .AnyAsync(pcm => pcm.PartyCategoryId == partyCategoryId);
        }
    }
}