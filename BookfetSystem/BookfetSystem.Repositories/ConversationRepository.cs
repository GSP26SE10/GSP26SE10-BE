using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class ConversationRepository : GenericRepository<Conversation>
    {
        public ConversationRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Conversation> GetAllConversationFiltered(Conversation filter)
        {
            var query = _context.Conversations
                .Include(c => c.Customer)
                .Include(c => c.Owner)
                .AsQueryable();

            if (filter.ConversationId != 0)
            {
                query = query.Where(c => c.ConversationId == filter.ConversationId);
            }

            if (filter.CustomerId != null)
            {
                query = query.Where(c => c.CustomerId == filter.CustomerId);
            }

            if (filter.OwnerId != null)
            {
                query = query.Where(c => c.OwnerId == filter.OwnerId);
            }

            return query.OrderByDescending(c => c.CreatedAt);
        }
    }
}
