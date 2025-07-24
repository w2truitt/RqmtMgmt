using backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface ITestPlanService
    {
        Task<IEnumerable<TestPlan>> GetAllAsync();
        Task<TestPlan> GetByIdAsync(int id);
        Task<TestPlan> CreateAsync(TestPlan plan);
        Task<TestPlan> UpdateAsync(TestPlan plan);
        Task<bool> DeleteAsync(int id);
    }
}
