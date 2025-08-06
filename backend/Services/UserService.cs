using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RqmtMgmtShared;
using System.ComponentModel.DataAnnotations;

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
            // Validate email format
            if (!IsValidEmail(dto.Email))
                return null;

            // Check for duplicate email
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return null;

            // Validate username
            if (string.IsNullOrWhiteSpace(dto.UserName))
                return null;

            var entity = FromDto(dto);
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        public async Task<bool> UpdateAsync(UserDto dto)
        {
            var tracked = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (tracked == null) return false;

            // Validate email format
            if (!IsValidEmail(dto.Email))
                return false;

            // Check for duplicate email (excluding current user)
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != dto.Id);
            if (existingUser != null)
                return false;

            // Validate username
            if (string.IsNullOrWhiteSpace(dto.UserName))
                return false;

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

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
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
            UserRoles = new List<UserRole>(),
            CreatedAt = DateTime.UtcNow
        };
    }
}