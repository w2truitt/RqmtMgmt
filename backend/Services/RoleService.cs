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

        public async Task<List<string>> GetAllRolesAsync()
        {
            return await _db.Roles.Select(r => r.Name).ToListAsync();
        }

        public async Task CreateRoleAsync(string roleName)
        {
            var exists = await _db.Roles.AnyAsync(r => r.Name.ToLower() == roleName.ToLower());
            if (!exists)
            {
                _db.Roles.Add(new Role { Name = roleName });
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteRoleAsync(string roleName)
        {
            var role = await _db.Roles.Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
            if (role == null || (role.UserRoles != null && role.UserRoles.Any()))
                return;
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
        }
    }
}
