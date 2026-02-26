using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;

namespace BookfetSystem.Repositories
{
    public class DishCategoryRepository : GenericRepository<DishCategory>
    {
        public DishCategoryRepository(GSP26SE10DBContext context) : base(context)
        {
        }
    }
}
