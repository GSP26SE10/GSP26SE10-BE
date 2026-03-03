using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class MenuCategoryRepository : GenericRepository<MenuCategory>
    {
        public MenuCategoryRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<MenuCategory> GetAllMenuCategoryFiltered(MenuCategory filter)
        {
            var query = _context.MenuCategories.AsQueryable();

            if (filter.MenuCategoryId != 0)
            {
                query = query.Where(mc => mc.MenuCategoryId == filter.MenuCategoryId);
            }

            if (!string.IsNullOrWhiteSpace(filter.MenuCategoryName))
            {
                query = query.Where(mc => mc.MenuCategoryName.ToLower().Contains(filter.MenuCategoryName.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Description))
            {
                query = query.Where(mc => mc.Description != null && mc.Description.ToLower().Contains(filter.Description.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(mc => mc.Status != null && mc.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            return query.OrderBy(mc => mc.MenuCategoryName);
        }

        public Task<bool> HasRelatedDataAsync(int menuCategoryId)
        {
            return _context.Menus.AnyAsync(m => m.MenuCategoryId == menuCategoryId);
        }
    }
}
