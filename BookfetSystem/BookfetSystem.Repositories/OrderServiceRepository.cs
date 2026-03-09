using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class OrderServiceRepository : GenericRepository<OrderService>
    {
        public OrderServiceRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<OrderService> GetAllOrderServiceFiltered(OrderService filter)
        {
            var query = _context.OrderServices
                .Include(x => x.OrderDetail)
                .Include(x => x.Service)
                .AsQueryable();

            if (filter.OrderServiceId != 0)
            {
                query = query.Where(x => x.OrderServiceId == filter.OrderServiceId);
            }

            if (filter.OrderDetailId != null)
            {
                query = query.Where(x => x.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.ServiceId != null)
            {
                query = query.Where(x => x.ServiceId == filter.ServiceId);
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
}
