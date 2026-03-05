using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.Repositories
{
    public class OrderRepository : GenericRepository<Order>
    {
        public OrderRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Order> GetAllFiltered(Order filter)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            if (filter.OrderId != 0)
                query = query.Where(o => o.OrderId == filter.OrderId);

            if (filter.CustomerId != null)
                query = query.Where(o => o.CustomerId == filter.CustomerId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(o => o.Status.Contains(filter.Status));

            if (filter.CreatedAt != null)
                query = query.Where(o => o.CreatedAt.Value.Date == filter.CreatedAt.Value.Date);

            return query.OrderByDescending(o => o.CreatedAt);
        }

        public async Task<bool> CheckCustomerExist(int customerId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == customerId);
        }

        public async Task<Order?> GetOrderWithDetailAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }
    }
}