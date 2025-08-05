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

            // Insert initial version record
            var version = new RequirementVersion
            {
                RequirementId = requirement.Id,
                Version = 1,
                Type = requirement.Type,
                Title = requirement.Title,
                Description = requirement.Description,
                ParentId = requirement.ParentId,
                Status = requirement.Status,
                ModifiedBy = requirement.CreatedBy,
                ModifiedAt = requirement.CreatedAt
            };
            _context.RequirementVersions.Add(version);
            await _context.SaveChangesAsync();
            return requirement;
        }

        public async Task<Requirement> UpdateAsync(Requirement updated)
        {
            var tracked = await _context.Requirements.FindAsync(updated.Id);
            if (tracked == null)
                throw new KeyNotFoundException($"Requirement with ID {updated.Id} not found.");

            // Save current state as new version BEFORE updating
            int nextVersion = await _context.RequirementVersions.CountAsync(v => v.RequirementId == tracked.Id) + 1;
            var version = new RequirementVersion
            {
                RequirementId = tracked.Id,
                Version = nextVersion,
                Type = tracked.Type,
                Title = tracked.Title,
                Description = tracked.Description,
                ParentId = tracked.ParentId,
                Status = tracked.Status,
                ModifiedBy = tracked.CreatedBy,
                ModifiedAt = tracked.UpdatedAt ?? DateTime.UtcNow
            };
            _context.RequirementVersions.Add(version);
            await _context.SaveChangesAsync();

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