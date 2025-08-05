using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class RoleService : IRoleService
    {
        private readonly RqmtMgmtDbContext _db;
        public RoleService(RqmtMgmtDbContext db) => _db = db;

        public Task<List<Role>> GetAllAsync() => _db.Roles.ToListAsync();
        public Task<Role?> GetByIdAsync(int id) => _db.Roles.FirstOrDefaultAsync(r => r.Id == id);
        public async Task<Role> CreateAsync(string name)
        {
            var existing = await _db.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
            if (existing != null)
                return existing;
            var role = new Role { Name = name };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return role;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _db.Roles.Include(r => r.UserRoles).FirstOrDefaultAsync(r => r.Id == id);
            if (role == null || (role.UserRoles != null && role.UserRoles.Any())) return false;
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
