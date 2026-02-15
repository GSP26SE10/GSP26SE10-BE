using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Repositories
{
    public class UserRepository : GenericRepository<User>
    {
        public UserRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public IQueryable<User> GetAllUserFiltered(User filter)
        {
            var query = _context.Users.AsQueryable();
            if (filter.UserId != 0)
                query = query.Where(u => u.UserId == filter.UserId);
            if (!string.IsNullOrEmpty(filter.FullName))
                query = query.Where(u => u.FullName.Contains(filter.FullName));
            if (!string.IsNullOrEmpty(filter.Address))
                query = query.Where(u => u.Address.Contains(filter.Address));
            if (!string.IsNullOrEmpty(filter.Email))
                query = query.Where(u => u.Email.Contains(filter.Email));
            if (!string.IsNullOrEmpty(filter.Phone))
                query = query.Where(u => u.Phone.Contains(filter.Phone));
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(u => u.Status.Contains(filter.Status));
            if (!string.IsNullOrEmpty(filter.UserName))
                query = query.Where(u => u.UserName.Contains(filter.UserName));
            if (filter.RoleId != null)
                query = query.Where(u => u.RoleId == filter.RoleId);
            return query.OrderByDescending(u => u.CreatedAt);
        }

        public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByUserName(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<bool> CheckRoleExistAsync(int roleId)
        {
            return await _context.Roles
                .AnyAsync(r => r.RoleId == roleId);
        }

    }
}
