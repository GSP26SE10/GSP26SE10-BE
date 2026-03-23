using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class FeedbackServiceRepository : GenericRepository<FeedbackService>
    {
        public FeedbackServiceRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<FeedbackService> GetAllFeedbackServiceFiltered(FeedbackService filter)
        {
            var query = _context.FeedbackServices
                .Include(fs => fs.Service)
                .Include(fs => fs.Customer)
                .AsQueryable();

            if (filter.FeedbackServiceId != 0)
            {
                query = query.Where(fs => fs.FeedbackServiceId == filter.FeedbackServiceId);
            }

            if (filter.OrderId.HasValue)
            {
                query = query.Where(fs => fs.OrderId == filter.OrderId);
            }

            if (filter.OrderDetailId.HasValue)
            {
                query = query.Where(fs => fs.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.ServiceId.HasValue)
            {
                query = query.Where(fs => fs.ServiceId == filter.ServiceId);
            }

            if (filter.CustomerId.HasValue)
            {
                query = query.Where(fs => fs.CustomerId == filter.CustomerId);
            }

            if (filter.Rating > 0)
            {
                query = query.Where(fs => fs.Rating == filter.Rating);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(fs => fs.Status != null && fs.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Comment))
            {
                query = query.Where(fs => fs.Comment != null && fs.Comment.ToLower().Contains(filter.Comment.ToLower()));
            }

            return query.OrderByDescending(fs => fs.CreatedAt);
        }
    }
}
