using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class MessageRepository : GenericRepository<Message>
    {
        public MessageRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Message> GetAllMessageFiltered(Message filter)
        {
            var query = _context.Messages
                .Include(m => m.Conversation)
                .Include(m => m.Sender)
                .AsQueryable();

            if (filter.MessageId != 0)
            {
                query = query.Where(m => m.MessageId == filter.MessageId);
            }

            if (filter.ConversationId != null)
            {
                query = query.Where(m => m.ConversationId == filter.ConversationId);
            }

            if (filter.SenderId != null)
            {
                query = query.Where(m => m.SenderId == filter.SenderId);
            }

            if (!string.IsNullOrEmpty(filter.Content))
            {
                query = query.Where(m => m.Content != null && m.Content.Contains(filter.Content));
            }


            return query.OrderByDescending(m => m.SentAt);
        }
    }
}
