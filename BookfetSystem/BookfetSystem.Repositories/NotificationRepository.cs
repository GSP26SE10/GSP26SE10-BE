using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>
    {
        public NotificationRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Notification> GetAllNotificationFiltered(Notification filter, int userId)
        {
            var query = _context.Notifications
                .Where(x => x.UserId == userId)
                .AsQueryable();

            if (filter.NotificationId != 0)
            {
                query = query.Where(x => x.NotificationId == filter.NotificationId);
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(x => x.Type == filter.Type);
            }

            if (filter.IsRead.HasValue)
            {
                query = query.Where(x => x.IsRead == filter.IsRead);
            }

            return query.OrderByDescending(x => x.CreatedAt);
        }

        public Task<Notification?> GetByIdAndUserIdAsync(int notificationId, int userId)
        {
            return _context.Notifications
                .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);
        }
    }
}