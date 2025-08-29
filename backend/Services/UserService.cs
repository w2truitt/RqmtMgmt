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
            var users = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsNoTracking().ToListAsync();
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
        /// Retrieves a specific user by their email address including assigned roles.
        /// </summary>
        /// <param name="EMAIL">The email address of the user.</param>
        /// <returns>The user DTO if found; otherwise, null.</returns>
        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Email == email);
            return user == null ? null : ToDto(user);
        }

        /// <summary>
        /// Gets the current user. This method is not implemented for the backend service.
        /// Use the UserController's /me endpoint instead.
        /// </summary>
        /// <returns>Throws NotImplementedException as this should not be called on the backend.</returns>
        /// <exception cref="NotImplementedException">Always thrown as this method is only for frontend use.</exception>
        public Task<UserDto?> GetCurrentUserAsync()
        {
            throw new NotImplementedException("GetCurrentUserAsync is not implemented for backend UserService. Use UserController /me endpoint instead.");
        }

        /// <summary>
        /// Creates a new user with validation for email format and uniqueness.
        /// </summary>
        /// <param name="user">The user data to create.</param>
        /// <returns>The created user DTO if successful; otherwise, null.</returns>
        public async Task<UserDto?> CreateAsync(UserDto user)
        {
            // Validate email format using built-in email validation
            if (!IsValidEmail(user.Email))
                return null;

            // Check for duplicate email to ensure uniqueness
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
                return null;

            // Validate username is not empty or whitespace
            if (string.IsNullOrWhiteSpace(user.UserName))
                return null;

            var entity = FromDto(user);
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            
                // Reload the entity with UserRoles to ensure navigation property is populated
                var createdEntity = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == entity.Id);
            
                return createdEntity == null ? null : ToDto(createdEntity);
        }

        /// <summary>
        /// Updates an existing user with validation for email format and uniqueness.
        /// Also updates user role assignments.
        /// </summary>
        /// <param name="user">The user data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(UserDto user)
        {
            var tracked = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == user.Id);
            if (tracked == null) return false;

            if (!await ValidateUserUpdateAsync(user))
                return false;

            UpdateUserProperties(tracked, user);
            await UpdateUserRolesAsync(tracked, user);

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> ValidateUserUpdateAsync(UserDto user)
        {
            // Validate email format using built-in email validation
            if (!IsValidEmail(user.Email))
                return false;

            // Check for duplicate email (excluding current user)
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email && u.Id != user.Id);
            if (existingUser != null)
                return false;

            // Validate username is not empty or whitespace
            if (string.IsNullOrWhiteSpace(user.UserName))
                return false;

            return true;
        }

        private static void UpdateUserProperties(User tracked, UserDto user)
        {
            tracked.UserName = user.UserName;
            tracked.Email = user.Email;
        }

        private async Task UpdateUserRolesAsync(User tracked, UserDto user)
        {
            // Update user roles if provided
            if (user.Roles == null) return;

            // Remove existing roles
            var currentRoles = tracked.UserRoles.ToList();
            _context.UserRoles.RemoveRange(currentRoles);

            // Add new roles
            foreach (var roleName in user.Roles)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    tracked.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
                }
            }
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
        /// Retrieves users with pagination, filtering, and sorting capabilities.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing users and pagination metadata.</returns>
        public async Task<PagedResult<UserDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var query = _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(u => u.UserName.Contains(parameters.SearchTerm) || 
                                        u.Email.Contains(parameters.SearchTerm));
            }

            // Get total count for pagination metadata
            var totalItems = await query.CountAsync();

            // Apply sorting
            query = !string.IsNullOrWhiteSpace(parameters.SortBy) ? parameters.SortBy.ToUpperInvariant() switch
            {
                "USERNAME" => parameters.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
                "EMAIL" => parameters.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "CREATEDAT" => parameters.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                _ => query.OrderBy(u => u.UserName)
            } : query.OrderBy(u => u.UserName);

            // Apply pagination
            var users = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<UserDto>
            {
                Items = users.Select(ToDto).ToList(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = totalItems
            };
        }

        /// <summary>
        /// Validates email format using .NET's built-in MailAddress validation.
        /// </summary>
        /// <param name="EMAIL">The email address to validate.</param>
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
        private static User FromDto(UserDto user) => new User
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            UserRoles = new List<UserRole>(),
            CreatedAt = DateTime.UtcNow
        };
    }
}