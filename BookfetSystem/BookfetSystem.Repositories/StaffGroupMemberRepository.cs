using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookfetSystem.Repositories
{
    public class StaffGroupMemberRepository : GenericRepository<StaffGroupMember>
    {
        public StaffGroupMemberRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<StaffGroupMember> GetAllStaffGroupMemberFiltered(StaffGroupMember filter)
        {
            var query = _context.StaffGroupMembers
                .Include(m => m.Staff)
                .Include(m => m.StaffGroup)
                .AsQueryable();

            if (filter.StaffGroupMemberId != 0)
            {
                query = query.Where(m => m.StaffGroupMemberId == filter.StaffGroupMemberId);
            }

            if (filter.StaffGroupId != null)
            {
                query = query.Where(m => m.StaffGroupId == filter.StaffGroupId);
            }

            if (filter.StaffId != null)
            {
                query = query.Where(m => m.StaffId == filter.StaffId);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(m => m.Status.Contains(filter.Status));
            }

            return query.OrderBy(m => m.StaffGroupMemberId);
        }

        public async Task<bool> ExistsAsync(int staffGroupId, int staffId, int? excludeMemberId = null)
        {
            var query = _context.StaffGroupMembers
                .Where(m => m.StaffGroupId == staffGroupId && m.StaffId == staffId);

            if (excludeMemberId.HasValue)
            {
                query = query.Where(m => m.StaffGroupMemberId != excludeMemberId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Check if staff is already a member of any group. Used to enforce "staff can only be in one group" rule.
        /// </summary>
        public async Task<bool> IsStaffInAnyGroupAsync(int staffId, int? excludeMemberId = null)
        {
            var query = _context.StaffGroupMembers.Where(m => m.StaffId == staffId);

            if (excludeMemberId.HasValue)
            {
                query = query.Where(m => m.StaffGroupMemberId != excludeMemberId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
