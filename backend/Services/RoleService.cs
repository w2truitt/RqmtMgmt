using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RqmtMgmtShared;

namespace backend.Services
{
    public class RoleService : IRoleService
    {
        private readonly RqmtMgmtDbContext _db;
        public RoleService(RqmtMgmtDbContext db) => _db = db;

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            return await _db.Roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name }).ToListAsync();
        }

        public async Task<RoleDto?> CreateRoleAsync(string roleName)
        {
            // Validate role name
            if (string.IsNullOrWhiteSpace(roleName))
                return null;

            var existing = await _db.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
            if (existing != null)
            {
                return new RoleDto { Id = existing.Id, Name = existing.Name };
            }

            var role = new Role { Name = roleName };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return new RoleDto { Id = role.Id, Name = role.Name };
        }

        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            var role = await _db.Roles.Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null || (role.UserRoles != null && role.UserRoles.Any()))
                return false;
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}