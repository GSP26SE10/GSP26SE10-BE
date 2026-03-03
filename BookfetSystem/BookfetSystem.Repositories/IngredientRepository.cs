using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class IngredientRepository : GenericRepository<Ingredient>
    {
        public IngredientRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Ingredient> GetAllIngredientFiltered(Ingredient filter)
        {
            var query = _context.Ingredients.AsQueryable();

            if (filter.IngredientId != 0)
            {
                query = query.Where(i => i.IngredientId == filter.IngredientId);
            }

            if (!string.IsNullOrWhiteSpace(filter.IngredientName))
            {
                query = query.Where(i => i.IngredientName.ToLower().Contains(filter.IngredientName.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Description))
            {
                query = query.Where(i => i.Description != null && i.Description.ToLower().Contains(filter.Description.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Img))
            {
                query = query.Where(i => i.Img != null && i.Img.ToLower().Contains(filter.Img.ToLower()));
            }

            return query.OrderBy(i => i.IngredientName);
        }

        public Task<bool> HasRelatedDataAsync(int ingredientId)
        {
            return _context.DishDetails.AnyAsync(dd => dd.IngredientId == ingredientId);
        }
    }
}
