using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class DishRepository : GenericRepository<Dish>
    {
        public DishRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Dish> GetAllDishFiltered(Dish filter, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Dishes
                .Include(d => d.DishCategory)
                .AsQueryable();

            if (filter.DishId != 0)
            {
                query = query.Where(d => d.DishId == filter.DishId);
            }

            if (!string.IsNullOrWhiteSpace(filter.DishName))
            {
                query = query.Where(d => d.DishName.ToLower().Contains(filter.DishName.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(d => d.Status != null && d.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            if (filter.DishCategoryId.HasValue)
            {
                query = query.Where(d => d.DishCategoryId == filter.DishCategoryId);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(d => d.Price.HasValue && d.Price.Value >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(d => d.Price.HasValue && d.Price.Value <= maxPrice.Value);
            }

            return query.OrderBy(d => d.DishName);
        }

        public async Task<bool> HasRelatedDataAsync(int dishId)
        {
            var hasMenuDish = await _context.MenuDishes.AnyAsync(md => md.DishId == dishId);
            if (hasMenuDish)
            {
                return true;
            }

            var hasDishDetail = await _context.DishDetails.AnyAsync(dd => dd.DishId == dishId);
            if (hasDishDetail)
            {
                return true;
            }

            return await _context.OrderDetailCustoms.AnyAsync(odc => odc.DishId == dishId);
        }
    }
}
