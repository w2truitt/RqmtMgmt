using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using RqmtMgmtShared;
using System.ComponentModel.DataAnnotations;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing users and their role assignments using the database context.
    /// Provides CRUD operations with validation for users and role management functionality.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly RqmtMgmtDbContext _context;

        /// <summary>
        /// Initializes a new instance of the UserService with the specified database context.
        /// </summary>
        /// <param name="context">The database context for user operations.</param>
        public UserService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all users from the database including their assigned roles.
        /// </summary>
        /// <returns>A list of all users as DTOs with their role information.</returns>
        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ToListAsync();
            return users.Select(ToDto).ToList();
        }

        /// <summary>
        /// Retrieves a specific user by their ID including assigned roles.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>The user DTO if found; otherwise, null.</returns>
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == id);
            return user == null ? null : ToDto(user);
        }

        /// <summary>
        /// Creates a new user with validation for email format and uniqueness.
        /// </summary>
        /// <param name="dto">The user data to create.</param>
        /// <returns>The created user DTO if successful; otherwise, null.</returns>
        public async Task<UserDto?> CreateAsync(UserDto dto)
        {
            // Validate email format using built-in email validation
            if (!IsValidEmail(dto.Email))
                return null;

            // Check for duplicate email to ensure uniqueness
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return null;

            // Validate username is not empty or whitespace
            if (string.IsNullOrWhiteSpace(dto.UserName))
                return null;

            var entity = FromDto(dto);
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return ToDto(entity);
        }

        /// <summary>
        /// Updates an existing user with validation for email format and uniqueness.
        /// </summary>
        /// <param name="dto">The user data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(UserDto dto)
        {
            var tracked = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (tracked == null) return false;

            // Validate email format using built-in email validation
            if (!IsValidEmail(dto.Email))
                return false;

            // Check for duplicate email (excluding current user)
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != dto.Id);
            if (existingUser != null)
                return false;

            // Validate username is not empty or whitespace
            if (string.IsNullOrWhiteSpace(dto.UserName))
                return false;

            tracked.UserName = dto.UserName;
            tracked.Email = dto.Email;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a user by their ID. Associated user roles are cascade deleted.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves all roles assigned to a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A list of role names assigned to the user, or an empty list if user not found.</returns>
        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId);
            return user?.UserRoles.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Assigns a role to a user, creating the role if it doesn't exist.
        /// Prevents duplicate role assignments.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The name of the role to assign.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AssignRoleAsync(int userId, string role)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;
            
            // Find or create the role
            var r = await _context.Roles.FirstOrDefaultAsync(x => x.Name == role);
            if (r == null)
            {
                r = new Role { Name = role };
                _context.Roles.Add(r);
                await _context.SaveChangesAsync();
            }
            
            // Assign role if not already assigned
            if (!user.UserRoles.Any(ur => ur.RoleId == r.Id))
            {
                user.UserRoles.Add(new UserRole { UserId = userId, RoleId = r.Id });
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Removes a role from a user if the assignment exists.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="role">The name of the role to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Validates email format using .NET's built-in MailAddress validation.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email format is valid; otherwise, false.</returns>
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

        /// <summary>
        /// Converts a User entity to a UserDto for API responses.
        /// Includes mapping of associated roles.
        /// </summary>
        /// <param name="u">The user entity to convert.</param>
        /// <returns>A UserDto with all properties and roles mapped.</returns>
        private static UserDto ToDto(User u) => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            Roles = u.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
        };

        /// <summary>
        /// Converts a UserDto to a User entity for database operations.
        /// Sets creation timestamp to current UTC time.
        /// </summary>
        /// <param name="dto">The user DTO to convert.</param>
        /// <returns>A User entity with all properties mapped.</returns>
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