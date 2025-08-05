using backend.Data;
using RqmtMgmtShared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class RequirementTestCaseLinkService : IRequirementTestCaseLinkService
    {
        private readonly RqmtMgmtDbContext _db;
        public RequirementTestCaseLinkService(RqmtMgmtDbContext db) => _db = db;

        public Task<List<RequirementTestCaseLinkDto>> GetLinksForRequirementAsync(int requirementId) =>
            Task.FromResult(
                _db.RequirementTestCaseLinks
                    .Where(l => l.RequirementId == requirementId)
                    .Select(l => new RequirementTestCaseLinkDto { RequirementId = l.RequirementId, TestCaseId = l.TestCaseId })
                    .ToList());

        public Task<List<RequirementTestCaseLinkDto>> GetLinksForTestCaseAsync(int testCaseId) =>
            Task.FromResult(
                _db.RequirementTestCaseLinks
                    .Where(l => l.TestCaseId == testCaseId)
                    .Select(l => new RequirementTestCaseLinkDto { RequirementId = l.RequirementId, TestCaseId = l.TestCaseId })
                    .ToList());

        public async Task<bool> CreateLinkAsync(RequirementTestCaseLinkDto dto)
        {
            var exists = _db.RequirementTestCaseLinks.Any(l => l.RequirementId == dto.RequirementId && l.TestCaseId == dto.TestCaseId);
            if (exists) return true; // silently succeed (idempotent)
            _db.RequirementTestCaseLinks.Add(new backend.Models.RequirementTestCaseLink { RequirementId = dto.RequirementId, TestCaseId = dto.TestCaseId });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLinkAsync(int requirementId, int testCaseId)
        {
            var link = _db.RequirementTestCaseLinks.FirstOrDefault(l => l.RequirementId == requirementId && l.TestCaseId == testCaseId);
            if (link == null) return false;
            _db.RequirementTestCaseLinks.Remove(link);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
