using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;

namespace BookfetSystem.Repositories
{
    public class DishRepository : GenericRepository<Dish>
    {
        public DishRepository(GSP26SE10DBContext context) : base(context)
        {
        }
    }
}
