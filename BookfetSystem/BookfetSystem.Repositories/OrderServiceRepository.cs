using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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

            if (filter.OrderDetailId.HasValue && filter.OrderDetailId.Value != 0)
            {
                query = query.Where(x => x.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.ServiceId.HasValue && filter.ServiceId.Value != 0)
            {
                query = query.Where(x => x.ServiceId == filter.ServiceId);
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }

        public async Task<OrderService?> GetByIdWithRelationAsync(int id)
        {
            return await _context.OrderServices
                .Include(x => x.OrderDetail)
                .Include(x => x.Service)
                .FirstOrDefaultAsync(x => x.OrderServiceId == id);
        }

        public Task<bool> HasRelatedDataAsync(int id)
        {
            return _context.OrderServices.AnyAsync(x => x.OrderServiceId == id);
        }
    }
}