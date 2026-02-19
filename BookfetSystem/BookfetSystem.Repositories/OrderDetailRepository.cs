using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;

namespace BookfetSystem.Repositories
{
    public class OrderDetailRepository : GenericRepository<OrderDetail>
    {
        public OrderDetailRepository(GSP26SE10DBContext context) : base(context)
        {
        }
    }
}
