using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class DishCategoryRepository : GenericRepository<DishCategory>
    {
        public DishCategoryRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<DishCategory> GetAllDishCategoryFiltered(DishCategory filter)
        {
            var query = _context.DishCategories.AsQueryable();

            if (filter.DishCategoryId != 0)
            {
                query = query.Where(dc => dc.DishCategoryId == filter.DishCategoryId);
            }

            if (!string.IsNullOrWhiteSpace(filter.DishCategoryName))
            {
                query = query.Where(dc => dc.DishCategoryName.ToLower().Contains(filter.DishCategoryName.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Description))
            {
                query = query.Where(dc => dc.Description != null && dc.Description.ToLower().Contains(filter.Description.ToLower()));
            }

            return query.OrderBy(dc => dc.DishCategoryName);
        }

        public Task<bool> HasRelatedDataAsync(int dishCategoryId)
        {
            return _context.Dishes.AnyAsync(d => d.DishCategoryId == dishCategoryId);
        }
    }
}
