using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;

namespace BookfetSystem.Repositories
{
    public class PartyCategoryRepository : GenericRepository<PartyCategory>
    {
        public PartyCategoryRepository(GSP26SE10DBContext context) : base(context)
        {
        }
    }
}
