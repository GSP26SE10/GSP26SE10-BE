using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class PostBlockRepository : GenericRepository<PostBlock>
    {
        public PostBlockRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<PostBlock> GetAllPostBlockFiltered(PostBlock filter)
        {
            var query = _context.PostBlocks
                .Include(pb => pb.Post)
                .AsQueryable();

            if (filter.PostBlockId != 0)
            {
                query = query.Where(pb => pb.PostBlockId == filter.PostBlockId);
            }

            if (filter.PostId != 0)
            {
                query = query.Where(pb => pb.PostId == filter.PostId);
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(pb => pb.Type.ToLower().Contains(filter.Type.ToLower()));
            }

            return query.OrderBy(pb => pb.Position);
        }
    }
}
