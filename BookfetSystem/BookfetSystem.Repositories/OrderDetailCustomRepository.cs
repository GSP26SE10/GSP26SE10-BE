using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class OrderDetailCustomRepository : GenericRepository<OrderDetailCustom>
    {
        public OrderDetailCustomRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<OrderDetailCustom> GetAllOrderDetailCustomFiltered(OrderDetailCustom filter)
        {
            var query = _context.OrderDetailCustoms
                .Include(x => x.Dish)
                .Include(x => x.OrderDetail)
                .AsQueryable();

            if (filter.OrderDetailCustomId != 0)
            {
                query = query.Where(x => x.OrderDetailCustomId == filter.OrderDetailCustomId);
            }

            if (filter.OrderDetailId != null)
            {
                query = query.Where(x => x.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.DishId != null)
            {
                query = query.Where(x => x.DishId == filter.DishId);
            }

            return query.OrderByDescending(x => x.OrderDetailCustomId);
        }
    }
}
