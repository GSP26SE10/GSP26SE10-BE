using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class PartyCategoryMenuRepository : GenericRepository<PartyCategoryMenu>
    {
        public PartyCategoryMenuRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<PartyCategoryMenu> GetAllPartyCategoryMenuFiltered(PartyCategoryMenu filter)
        {
            var query = _context.PartyCategoryMenus
                .Include(pcm => pcm.PartyCategory)
                .Include(pcm => pcm.Menu)
                .AsQueryable();

            if (filter.PartyCategoryMenuId != 0)
            {
                query = query.Where(pcm => pcm.PartyCategoryMenuId == filter.PartyCategoryMenuId);
            }

            if (filter.PartyCategoryId.HasValue)
            {
                query = query.Where(pcm => pcm.PartyCategoryId == filter.PartyCategoryId);
            }

            if (filter.MenuId.HasValue)
            {
                query = query.Where(pcm => pcm.MenuId == filter.MenuId);
            }

            return query.OrderBy(pcm => pcm.PartyCategoryMenuId);
        }

        public async Task<bool> ExistsAsync(int partyCategoryId, int menuId, int? excludePartyCategoryMenuId = null)
        {
            var query = _context.PartyCategoryMenus
                .Where(pcm => pcm.PartyCategoryId == partyCategoryId && pcm.MenuId == menuId);

            if (excludePartyCategoryMenuId.HasValue)
            {
                query = query.Where(pcm => pcm.PartyCategoryMenuId != excludePartyCategoryMenuId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
