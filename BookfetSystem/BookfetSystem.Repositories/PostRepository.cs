using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class PostRepository : GenericRepository<Post>
    {
        public PostRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<Post> GetAllPostFiltered(Post filter)
        {
            var query = _context.Posts
                .Include(p => p.BlogCategory)
                .AsQueryable();

            if (filter.PostId != 0)
            {
                query = query.Where(p => p.PostId == filter.PostId);
            }

            if (!string.IsNullOrWhiteSpace(filter.Slug))
            {
                query = query.Where(p => p.Slug.ToLower().Contains(filter.Slug.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                query = query.Where(p => p.Title.ToLower().Contains(filter.Title.ToLower()));
            }

            if (filter.BlogCategoryId.HasValue && filter.BlogCategoryId.Value != 0)
            {
                query = query.Where(p => p.BlogCategoryId == filter.BlogCategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(p => p.Status != null && p.Status.ToLower().Contains(filter.Status.ToLower()));
            }

            return query.OrderByDescending(p => p.CreatedAt);
        }

        public Task<bool> HasRelatedDataAsync(int postId)
        {
            return _context.PostBlocks.AnyAsync(pb => pb.PostId == postId);
        }

        public async Task<Post?> GetByIdWithBlogCategoryAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.BlogCategory)
                .FirstOrDefaultAsync(p => p.PostId == id);
        }
    }
}
