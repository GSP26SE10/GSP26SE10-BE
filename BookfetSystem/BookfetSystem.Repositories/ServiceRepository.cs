using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;

namespace BookfetSystem.Repositories
{
    public class ServiceRepository : GenericRepository<Service>
    {
        public ServiceRepository(GSP26SE10DBContext context) : base(context)
        {
        }
    }
}
