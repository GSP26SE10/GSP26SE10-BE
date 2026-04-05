using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class DishDetailRepository : GenericRepository<DishDetail>
    {
        public DishDetailRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<DishDetail> GetAllDishDetailFiltered(DishDetail filter)
        {
            var query = _context.DishDetails
                .Include(dd => dd.Dish)
                .Include(dd => dd.Ingredient)
                .AsQueryable();

            if (filter.DishDetailId != 0)
            {
                query = query.Where(dd => dd.DishDetailId == filter.DishDetailId);
            }

            if (filter.DishId.HasValue)
            {
                query = query.Where(dd => dd.DishId == filter.DishId);
            }

            if (filter.IngredientId.HasValue)
            {
                query = query.Where(dd => dd.IngredientId == filter.IngredientId);
            }

            return query.OrderBy(dd => dd.DishDetailId);
        }

        public async Task<bool> ExistsAsync(int dishId, int ingredientId, int? excludeDishDetailId = null)
        {
            var query = _context.DishDetails
                .Where(dd => dd.DishId == dishId && dd.IngredientId == ingredientId);

            if (excludeDishDetailId.HasValue)
            {
                query = query.Where(dd => dd.DishDetailId != excludeDishDetailId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
