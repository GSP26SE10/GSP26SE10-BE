using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;

namespace BookfetSystem.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>
    {
        public NotificationRepository(GSP26SE10DBContext context) : base(context)
        {
        }
    }
}