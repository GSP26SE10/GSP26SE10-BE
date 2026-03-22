using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class MenuRepository : GenericRepository<Menu>
    {
        public MenuRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Menu> GetAllMenuFiltered(Menu filter, decimal? minBasePrice, decimal? maxBasePrice)
        {
            var query = _context.Menus
                .Include(m => m.MenuCategory)
                .Include(m => m.PartyCategoryMenus)
                .ThenInclude(pcm => pcm.PartyCategory)
                .AsQueryable();

            if (filter.MenuId != 0)
            {
                query = query.Where(m => m.MenuId == filter.MenuId);
            }

            if (filter.MenuCategoryId.HasValue && filter.MenuCategoryId.Value != 0)
            {
                query = query.Where(m => m.MenuCategoryId == filter.MenuCategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.MenuName))
            {
                query = query.Where(m => m.MenuName.ToLower().Contains(filter.MenuName.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(m => m.Status == filter.Status);
            }

            if (minBasePrice.HasValue)
            {
                query = query.Where(m => m.BasePrice.HasValue && m.BasePrice.Value >= minBasePrice.Value);
            }

            if (maxBasePrice.HasValue)
            {
                query = query.Where(m => m.BasePrice.HasValue && m.BasePrice.Value <= maxBasePrice.Value);
            }

            return query.OrderBy(m => m.MenuName);
        }

        public async Task<bool> HasRelatedDataAsync(int menuId)
        {
            var hasMenuDish = await _context.MenuDishes.AnyAsync(md => md.MenuId == menuId);
            if (hasMenuDish)
            {
                return true;
            }

            var hasOrderDetail = await _context.OrderDetails.AnyAsync(od => od.MenuId == menuId);
            if (hasOrderDetail)
            {
                return true;
            }

            return await _context.PartyCategoryMenus.AnyAsync(pcm => pcm.MenuId == menuId);
        }

        public async Task<List<Menu>> GetAllWithRelationsAsync()
        {
            return await _context.Menus
                .Include(m => m.MenuDishes)
                    .ThenInclude(md => md.Dish)
                .ToListAsync();
        }
    }
}