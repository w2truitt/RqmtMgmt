using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RqmtMgmtShared;

namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly RqmtMgmtDbContext _context;
        public UserService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ToListAsync();
            return users.Select(ToDto).ToList();
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == id);
            return user == null ? null : ToDto(user);
        }

        public async Task<UserDto?> CreateAsync(UserDto dto)
        {
            var entity = FromDto(dto);
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        public async Task<bool> UpdateAsync(UserDto dto)
        {
            var tracked = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (tracked == null) return false;
            tracked.UserName = dto.UserName;
            tracked.Email = dto.Email;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId);
            return user?.UserRoles.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
        }

        public async Task AssignRoleAsync(int userId, string role)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;
            var r = await _context.Roles.FirstOrDefaultAsync(x => x.Name == role);
            if (r == null)
            {
                r = new Role { Name = role };
                _context.Roles.Add(r);
                await _context.SaveChangesAsync();
            }
            if (!user.UserRoles.Any(ur => ur.RoleId == r.Id))
            {
                user.UserRoles.Add(new UserRole { UserId = userId, RoleId = r.Id });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveRoleAsync(int userId, string role)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;
            var userRole = user.UserRoles.FirstOrDefault(ur => ur.Role.Name == role);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        // Mapping helpers
        private static UserDto ToDto(User u) => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            Roles = u.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
        };

        private static User FromDto(UserDto dto) => new User
        {
            Id = dto.Id,
            UserName = dto.UserName,
            Email = dto.Email,
            UserRoles = dto.Roles?.Select(r => new UserRole { Role = new Role { Name = r } }).ToList() ?? new List<UserRole>()
        };
    }
}
