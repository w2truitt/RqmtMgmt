using RqmtMgmtShared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IRequirementTestCaseLinkService
    {
        Task<List<RequirementTestCaseLinkDto>> GetLinksForRequirementAsync(int requirementId);
        Task<List<RequirementTestCaseLinkDto>> GetLinksForTestCaseAsync(int testCaseId);
        Task<bool> CreateLinkAsync(RequirementTestCaseLinkDto dto);
        Task<bool> DeleteLinkAsync(int requirementId, int testCaseId);
    }
}
