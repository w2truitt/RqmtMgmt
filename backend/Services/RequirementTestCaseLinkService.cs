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

        public Task<List<RequirementTestCaseLinkDto>> GetLinksForRequirement(int requirementId) =>
            Task.FromResult(
                _db.RequirementTestCaseLinks
                    .Where(l => l.RequirementId == requirementId)
                    .Select(l => new RequirementTestCaseLinkDto { RequirementId = l.RequirementId, TestCaseId = l.TestCaseId })
                    .ToList());

        public Task<List<RequirementTestCaseLinkDto>> GetLinksForTestCase(int testCaseId) =>
            Task.FromResult(
                _db.RequirementTestCaseLinks
                    .Where(l => l.TestCaseId == testCaseId)
                    .Select(l => new RequirementTestCaseLinkDto { RequirementId = l.RequirementId, TestCaseId = l.TestCaseId })
                    .ToList());

        public async Task AddLink(int requirementId, int testCaseId)
        {
            var exists = _db.RequirementTestCaseLinks.Any(l => l.RequirementId == requirementId && l.TestCaseId == testCaseId);
            if (!exists)
            {
                _db.RequirementTestCaseLinks.Add(new backend.Models.RequirementTestCaseLink { RequirementId = requirementId, TestCaseId = testCaseId });
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveLink(int requirementId, int testCaseId)
        {
            var link = _db.RequirementTestCaseLinks.FirstOrDefault(l => l.RequirementId == requirementId && l.TestCaseId == testCaseId);
            if (link != null)
            {
                _db.RequirementTestCaseLinks.Remove(link);
                await _db.SaveChangesAsync();
            }
        }
    }
}
