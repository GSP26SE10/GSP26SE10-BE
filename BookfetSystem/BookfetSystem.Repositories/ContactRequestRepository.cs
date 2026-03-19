using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class ContactRequestRepository : GenericRepository<ContactRequest>
    {
        public ContactRequestRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<ContactRequest> GetAllFiltered(ContactRequest filter)
        {
            var query = _context.ContactRequests
                .Include(x => x.Customer)
                .AsQueryable();

            if (filter.ContactRequestId != 0)
            {
                query = query.Where(x => x.ContactRequestId == filter.ContactRequestId);
            }

            if (filter.CustomerId.HasValue && filter.CustomerId.Value != 0)
            {
                query = query.Where(x => x.CustomerId == filter.CustomerId);
            }

            if (!string.IsNullOrWhiteSpace(filter.FullName))
            {
                query = query.Where(x => x.FullName.ToLower().Contains(filter.FullName.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                query = query.Where(x => x.Email.ToLower().Contains(filter.Email.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Phone))
            {
                query = query.Where(x => x.Phone.Contains(filter.Phone));
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(x => x.Status == filter.Status);
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
}