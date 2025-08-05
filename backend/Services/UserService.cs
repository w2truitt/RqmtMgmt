using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly RqmtMgmtDbContext _context;
        public UserService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User updated)
        {
            var tracked = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == updated.Id);
            if (tracked == null)
                throw new KeyNotFoundException($"User with ID {updated.Id} not found.");

            tracked.UserName = updated.UserName;
            tracked.Email = updated.Email;
            // tracked.Roles: Roles are managed via AssignRoles/RemoveRole

            await _context.SaveChangesAsync();
            return tracked;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> AssignRolesAsync(int userId, List<string> roles)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            foreach (var roleName in roles.Distinct())
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                {
                    role = new Role { Name = roleName };
                    _context.Roles.Add(role);
                    await _context.SaveChangesAsync();
                }
                // Only add if not already assigned
                if (!user.UserRoles.Any(ur => ur.RoleId == role.Id))
                {
                    user.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRoleAsync(int userId, string roleName)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;
            var userRole = user.UserRoles.FirstOrDefault(ur => ur.Role.Name.Equals(roleName, System.StringComparison.OrdinalIgnoreCase));
            if (userRole == null) return false;
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
