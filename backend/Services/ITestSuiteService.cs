using backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface ITestSuiteService
    {
        Task<IEnumerable<TestSuite>> GetAllAsync();
        Task<TestSuite?> GetByIdAsync(int id);
        Task<TestSuite> CreateAsync(TestSuite suite);
        Task<TestSuite> UpdateAsync(TestSuite suite);
        Task<bool> DeleteAsync(int id);
    }
}
