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
    public class RequirementService : RqmtMgmtShared.IRequirementService
    {
        private readonly RqmtMgmtDbContext _context;

        public RequirementService(RqmtMgmtDbContext context)
        {
            _context = context;
        }


        public async Task<List<RequirementDto>> GetAllAsync()
        {
            var entities = await _context.Requirements.ToListAsync();
            return entities.Select(EntityToDto).ToList();
        }

        public async Task<RequirementDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Requirements.FindAsync(id);
            return entity == null ? null : EntityToDto(entity);
        }

        public async Task<RequirementDto?> CreateAsync(RequirementDto dto)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
                return null;
            
            if (dto.CreatedBy <= 0)
                return null;
            
            // Check for circular reference if ParentId is provided
            if (dto.ParentId.HasValue && await WouldCreateCircularReference(dto.Id, dto.ParentId.Value))
                return null;

            var entity = DtoToEntity(dto);
            _context.Requirements.Add(entity);
            await _context.SaveChangesAsync();

            // Save initial version
            var version = new RequirementVersion
            {
                RequirementId = entity.Id,
                Version = 1,
                Type = entity.Type,
                Title = entity.Title,
                Description = entity.Description,
                ParentId = entity.ParentId,
                Status = entity.Status,
                ModifiedBy = entity.CreatedBy,
                ModifiedAt = entity.CreatedAt
            };
            _context.RequirementVersions.Add(version);
            await _context.SaveChangesAsync();
            return EntityToDto(entity);
        }

        public async Task<bool> UpdateAsync(RequirementDto dto)
        {
            var entity = await _context.Requirements.FindAsync(dto.Id);
            if (entity == null)
                return false;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
                return false;
            
            if (dto.CreatedBy <= 0)
                return false;
            
            // Check for circular reference if ParentId is provided
            if (dto.ParentId.HasValue && await WouldCreateCircularReference(dto.Id, dto.ParentId.Value))
                return false;

            // Save current state as new version BEFORE updating
            int nextVersion = await _context.RequirementVersions.CountAsync(v => v.RequirementId == entity.Id) + 1;
            var version = new RequirementVersion
            {
                RequirementId = entity.Id,
                Version = nextVersion,
                Type = entity.Type,
                Title = entity.Title,
                Description = entity.Description,
                ParentId = entity.ParentId,
                Status = entity.Status,
                ModifiedBy = dto.CreatedBy, // Use the user making the update
                ModifiedAt = DateTime.UtcNow
            };
            _context.RequirementVersions.Add(version);

            // Update entity from DTO (don't change CreatedBy and CreatedAt)
            entity.Type = dto.Type;
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.ParentId = dto.ParentId;
            entity.Status = dto.Status;
            entity.Version = nextVersion;
            entity.UpdatedAt = DateTime.UtcNow;

            // Save all changes in one transaction
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var req = await _context.Requirements.FindAsync(id);
            if (req == null) return false;
            _context.Requirements.Remove(req);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RequirementVersionDto>> GetVersionsAsync(int requirementId)
        {
            var versions = await _context.RequirementVersions
                .Where(v => v.RequirementId == requirementId)
                .OrderBy(v => v.Version)
                .ToListAsync();
            return versions.Select(EntityToVersionDto).ToList();
        }

        private async Task<bool> WouldCreateCircularReference(int requirementId, int parentId)
        {
            // Check if parentId is a descendant of requirementId
            var current = parentId;
            var visited = new HashSet<int>();
            
            while (current != 0 && !visited.Contains(current))
            {
                if (current == requirementId)
                    return true;
                
                visited.Add(current);
                var parent = await _context.Requirements.FindAsync(current);
                current = parent?.ParentId ?? 0;
            }
            
            return false;
        }

        private static RequirementDto EntityToDto(Requirement r) => new()
        {
            Id = r.Id,
            Type = r.Type,
            Title = r.Title,
            Description = r.Description,
            ParentId = r.ParentId,
            Status = r.Status,
            Version = r.Version,
            CreatedBy = r.CreatedBy,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };

        private static RequirementVersionDto EntityToVersionDto(RequirementVersion v) => new()
        {
            Id = v.Id,
            RequirementId = v.RequirementId,
            Version = v.Version,
            Title = v.Title,
            Description = v.Description,
            Type = v.Type,
            Status = v.Status,
            ModifiedBy = v.ModifiedBy,
            ModifiedAt = v.ModifiedAt
        };

        private static Requirement DtoToEntity(RequirementDto d) => new()
        {
            Id = d.Id,
            Type = d.Type,
            Title = d.Title,
            Description = d.Description,
            ParentId = d.ParentId,
            Status = d.Status,
            Version = d.Version,
            CreatedBy = d.CreatedBy,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };
    }
}