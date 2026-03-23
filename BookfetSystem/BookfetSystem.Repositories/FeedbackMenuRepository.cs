using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class FeedbackMenuRepository : GenericRepository<FeedbackMenu>
    {
        public FeedbackMenuRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<FeedbackMenu> GetAllFeedbackMenuFiltered(FeedbackMenu filter)
        {
            var query = _context.FeedbackMenus
                .Include(fm => fm.Menu)
                .Include(fm => fm.Customer)
                .AsQueryable();

            if (filter.FeedbackMenuId != 0)
            {
                query = query.Where(fm => fm.FeedbackMenuId == filter.FeedbackMenuId);
            }

            if (filter.OrderId.HasValue)
            {
                query = query.Where(fm => fm.OrderId == filter.OrderId);
            }

            if (filter.OrderDetailId.HasValue)
            {
                query = query.Where(fm => fm.OrderDetailId == filter.OrderDetailId);
            }

            if (filter.MenuId.HasValue)
            {
                query = query.Where(fm => fm.MenuId == filter.MenuId);
            }

            if (filter.CustomerId.HasValue)
            {
                query = query.Where(fm => fm.CustomerId == filter.CustomerId);
            }

            if (filter.Rating > 0)
            {
                query = query.Where(fm => fm.Rating == filter.Rating);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(fm => fm.Status != null && fm.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Comment))
            {
                query = query.Where(fm => fm.Comment != null && fm.Comment.ToLower().Contains(filter.Comment.ToLower()));
            }

            return query.OrderByDescending(fm => fm.CreatedAt);
        }
    }
}
