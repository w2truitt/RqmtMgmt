using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RqmtMgmtShared;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing user roles using the database context.
    /// Provides operations for creating, retrieving, and deleting roles with validation.
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly RqmtMgmtDbContext _db;

        /// <summary>
        /// Initializes a new instance of the RoleService with the specified database context.
        /// </summary>
        /// <param name="db">The database context for role operations.</param>
        public RoleService(RqmtMgmtDbContext db) => _db = db;

        /// <summary>
        /// Retrieves all roles from the database.
        /// </summary>
        /// <returns>A list of all roles as DTOs.</returns>
        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            return await _db.Roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name }).ToListAsync();
        }

        /// <summary>
        /// Creates a new role with the specified name, or returns existing role if it already exists.
        /// Performs case-insensitive duplicate checking to prevent role name conflicts.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <returns>The created or existing role DTO if successful; otherwise, null.</returns>
        public async Task<RoleDto?> CreateRoleAsync(string roleName)
        {
            // Validate role name is not empty or whitespace
            if (string.IsNullOrWhiteSpace(roleName))
                return null;

            // Check for existing role with case-insensitive comparison
            var existing = await _db.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
            if (existing != null)
            {
                return new RoleDto { Id = existing.Id, Name = existing.Name };
            }

            // Create new role if it doesn't exist
            var role = new Role { Name = roleName };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return new RoleDto { Id = role.Id, Name = role.Name };
        }

        /// <summary>
        /// Deletes a role by its ID if it's not assigned to any users.
        /// Prevents deletion of roles that are currently in use to maintain data integrity.
        /// </summary>
        /// <param name="roleId">The unique identifier of the role to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var role = await _db.Roles.Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == roleId);
            
            // Prevent deletion if role doesn't exist or is assigned to users
            if (role == null || (role.UserRoles != null && role.UserRoles.Any()))
                return false;
            
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}