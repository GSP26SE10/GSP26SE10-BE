using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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

            if (filter.OrderDetailId.HasValue && filter.OrderDetailId.Value != 0)
            {
                query = query.Where(x => x.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.DishId.HasValue && filter.DishId.Value != 0)
            {
                query = query.Where(x => x.DishId == filter.DishId);
            }

            if (filter.Quantity.HasValue && filter.Quantity.Value != 0)
            {
                query = query.Where(x => x.Quantity == filter.Quantity);
            }

            return query.OrderByDescending(x => x.OrderDetailCustomId);
        }

        public async Task<OrderDetailCustom?> GetByIdWithRelationAsync(int id)
        {
            return await _context.OrderDetailCustoms
                .Include(x => x.Dish)
                .Include(x => x.OrderDetail)
                .FirstOrDefaultAsync(x => x.OrderDetailCustomId == id);
        }

        public Task<bool> HasRelatedDataAsync(int id)
        {
            return _context.OrderDetailCustoms
                .AnyAsync(x => x.OrderDetailCustomId == id);
        }
    }
}