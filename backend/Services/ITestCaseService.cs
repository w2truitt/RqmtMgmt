using backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface ITestCaseService
    {
        Task<IEnumerable<TestCase>> GetAllAsync();
        Task<TestCase> GetByIdAsync(int id);
        Task<TestCase> CreateAsync(TestCase testCase);
        Task<TestCase> UpdateAsync(TestCase testCase);
        Task<bool> DeleteAsync(int id);
    }
}
