using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class BlogCategoryRepository : GenericRepository<BlogCategory>
    {
        public BlogCategoryRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<BlogCategory> GetAllBlogCategoryFiltered(BlogCategory filter)
        {
            var query = _context.BlogCategories.AsQueryable();

            if (filter.BlogCategoryId != 0)
            {
                query = query.Where(bc => bc.BlogCategoryId == filter.BlogCategoryId);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(bc => bc.Name.ToLower().Contains(filter.Name.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Slug))
            {
                query = query.Where(bc => bc.Slug.ToLower().Contains(filter.Slug.ToLower()));
            }

            return query.OrderBy(bc => bc.Name);
        }

        public Task<bool> HasRelatedDataAsync(int blogCategoryId)
        {
            return _context.Posts.AnyAsync(p => p.BlogCategoryId == blogCategoryId);
        }
    }
}
