using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using RqmtMgmtShared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    /// <summary>
    /// Service implementation for managing requirements using the database context.
    /// </summary>
    public class RequirementService : IRequirementService
    {
        private readonly RqmtMgmtDbContext _context;

        public RequirementService(RqmtMgmtDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Requirement>> GetAllAsync()
        {
            return await _context.Requirements.ToListAsync();
        }

        public async Task<Requirement?> GetByIdAsync(int id)
        {
            return await _context.Requirements.FindAsync(id);
        }

        public async Task<Requirement> CreateAsync(Requirement requirement)
        {
            _context.Requirements.Add(requirement);
            await _context.SaveChangesAsync();
            return requirement;
        }

        public async Task<Requirement> UpdateAsync(Requirement updated)
        {
            var tracked = await _context.Requirements.FindAsync(updated.Id);
            if (tracked == null)
                throw new KeyNotFoundException($"Requirement with ID {updated.Id} not found.");

            // Update properties
            tracked.Type = updated.Type;
            tracked.Title = updated.Title;
            tracked.Description = updated.Description;
            tracked.ParentId = updated.ParentId;
            tracked.Status = updated.Status;
            tracked.Version = updated.Version;
            tracked.CreatedBy = updated.CreatedBy;
            tracked.CreatedAt = updated.CreatedAt;
            tracked.UpdatedAt = updated.UpdatedAt;

            await _context.SaveChangesAsync();
            return tracked;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var req = await _context.Requirements.FindAsync(id);
            if (req == null) return false;
            _context.Requirements.Remove(req);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}