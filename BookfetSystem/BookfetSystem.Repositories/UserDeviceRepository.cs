using BookfetSystem.Repositories.Basic;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.Repositories
{
    public class UserDeviceRepository : GenericRepository<UserDevice>
    {
        public UserDeviceRepository(GSP26SE10DBContext context) : base(context)
        {
        }

        public async Task UpsertByDeviceIdAsync(int userId, string deviceId, string expoPushToken, string platform, bool isActive)
        {
            var now = DateTime.UtcNow;

            var deviceRecord = await _context.UserDevices
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.UserDeviceId)
                .FirstOrDefaultAsync(x => x.DeviceId == deviceId);

            var tokenRecord = await _context.UserDevices
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.UserDeviceId)
                .FirstOrDefaultAsync(x => x.ExpoPushToken == expoPushToken);

            var target = deviceRecord ?? tokenRecord;
            if (target == null)
            {
                target = new UserDevice
                {
                    UserId = userId,
                    DeviceId = deviceId,
                    ExpoPushToken = expoPushToken,
                    Platform = platform,
                    IsActive = isActive,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.UserDevices.Add(target);
            }
            else
            {
                target.UserId = userId;
                target.DeviceId = deviceId;
                target.ExpoPushToken = expoPushToken;
                target.Platform = platform;
                target.IsActive = isActive;
                target.UpdatedAt = now;
                target.CreatedAt ??= now;
            }

            // Keep one active row for the same device or token to avoid sending duplicates.
            var duplicatedRows = await _context.UserDevices
                .Where(x => x.UserDeviceId != target.UserDeviceId && (x.DeviceId == deviceId || x.ExpoPushToken == expoPushToken))
                .ToListAsync();

            foreach (var row in duplicatedRows)
            {
                row.IsActive = false;
                row.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> DeactivateByDeviceIdAsync(string deviceId)
        {
            var now = DateTime.UtcNow;
            var rows = await _context.UserDevices
                .Where(x => x.DeviceId == deviceId && x.IsActive == true)
                .ToListAsync();

            foreach (var row in rows)
            {
                row.IsActive = false;
                row.UpdatedAt = now;
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeactivateByExpoPushTokenAsync(string expoPushToken)
        {
            var now = DateTime.UtcNow;
            var rows = await _context.UserDevices
                .Where(x => x.ExpoPushToken == expoPushToken && x.IsActive == true)
                .ToListAsync();

            foreach (var row in rows)
            {
                row.IsActive = false;
                row.UpdatedAt = now;
            }

            return await _context.SaveChangesAsync();
        }

        public Task<List<string>> GetActiveTokensByUserIdAsync(int userId)
        {
            return _context.UserDevices
                .Where(x => x.UserId == userId && x.IsActive == true && !string.IsNullOrWhiteSpace(x.ExpoPushToken))
                .Select(x => x.ExpoPushToken)
                .Distinct()
                .ToListAsync();
        }
    }
}