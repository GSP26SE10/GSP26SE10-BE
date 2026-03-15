using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class StaffGroupRepository : GenericRepository<StaffGroup>
    {
        public StaffGroupRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<StaffGroup> GetAllStaffGroupFiltered(StaffGroup filter)
        {
            var query = _context.StaffGroups
                .Include(sg => sg.Leader)
                .AsQueryable();

            if (filter.StaffGroupId != 0)
            {
                query = query.Where(sg => sg.StaffGroupId == filter.StaffGroupId);
            }

            if (!string.IsNullOrEmpty(filter.StaffGroupName))
            {
                query = query.Where(sg => sg.StaffGroupName.Contains(filter.StaffGroupName));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(sg => sg.Status == filter.Status);
            }

            if (filter.LeaderId != null)
            {
                query = query.Where(sg => sg.LeaderId == filter.LeaderId);
            }

            return query.OrderBy(sg => sg.StaffGroupName);
        }

        public async Task<bool> HasOrderDetailsAsync(int staffGroupId)
        {
            return await _context.OrderDetails
                .AnyAsync(od => od.StaffGroupId == staffGroupId);
        }

        public async Task<StaffGroup?> GetAssignmentOverviewByLeaderIdAsync(int leaderId)
        {
            return await _context.StaffGroups
                .Include(sg => sg.Leader)
                .Include(sg => sg.StaffGroupMembers)
                    .ThenInclude(member => member.Staff)
                .Include(sg => sg.OrderDetails)
                    .ThenInclude(od => od.Menu)
                .Include(sg => sg.OrderDetails)
                    .ThenInclude(od => od.PartyCategory)
                .Include(sg => sg.OrderDetails)
                    .ThenInclude(od => od.OrderDetailStaffTasks)
                        .ThenInclude(task => task.Staff)
                .Where(sg => sg.LeaderId == leaderId)
                .OrderByDescending(sg => sg.StaffGroupId)
                .FirstOrDefaultAsync();
        }
    }
}

