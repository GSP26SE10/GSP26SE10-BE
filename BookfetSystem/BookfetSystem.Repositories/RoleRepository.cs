using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class RoleRepository : GenericRepository<Role>
    {
        public RoleRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Role> GetAllRoleFiltered(Role filter)
        {
            var query = _context.Roles.AsQueryable();

            if (filter.RoleId != 0)
            {
                query = query.Where(r => r.RoleId == filter.RoleId);
            }

            if (!string.IsNullOrEmpty(filter.RoleName))
            {
                query = query.Where(r => r.RoleName.ToLower().Contains(filter.RoleName.ToLower()));
            }

            return query.OrderBy(r => r.RoleName);
        }
    }
}
