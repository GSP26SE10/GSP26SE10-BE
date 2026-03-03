using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class MenuDishRepository : GenericRepository<MenuDish>
    {
        public MenuDishRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<MenuDish> GetAllMenuDishFiltered(MenuDish filter)
        {
            var query = _context.MenuDishes
                .Include(md => md.Menu)
                .Include(md => md.Dish)
                .AsQueryable();

            if (filter.MenuDishId != 0)
            {
                query = query.Where(md => md.MenuDishId == filter.MenuDishId);
            }

            if (filter.MenuId != null)
            {
                query = query.Where(md => md.MenuId == filter.MenuId);
            }

            if (filter.DishId != null)
            {
                query = query.Where(md => md.DishId == filter.DishId);
            }

            return query.OrderBy(md => md.MenuDishId);
        }

        public async Task<bool> ExistsAsync(int menuId, int dishId, int? excludeMenuDishId = null)
        {
            var query = _context.MenuDishes
                .Where(md => md.MenuId == menuId && md.DishId == dishId);

            if (excludeMenuDishId.HasValue)
            {
                query = query.Where(md => md.MenuDishId != excludeMenuDishId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
