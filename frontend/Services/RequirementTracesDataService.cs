using RqmtMgmtShared;
using System.Net.Http.Json;

namespace frontend.Services
{
    /// <summary>
    /// Service for managing requirement traces through the API.
    /// </summary>
    public class RequirementTracesDataService : BaseDataService
    {
        public RequirementTracesDataService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Gets outgoing traces from a source requirement.
        /// </summary>
        public async Task<List<RequirementTraceDto>> GetSourceTracesAsync(int sourceRequirementId)
        {
            return await GetListAsync<RequirementTraceDto>($"/api/requirementtraces/source/{sourceRequirementId}");
        }

        /// <summary>
        /// Gets incoming traces to a target requirement.
        /// </summary>
        public async Task<List<RequirementTraceDto>> GetTargetTracesAsync(int targetRequirementId)
        {
            return await GetListAsync<RequirementTraceDto>($"/api/requirementtraces/target/{targetRequirementId}");
        }

        /// <summary>
        /// Gets the complete trace chain for a requirement.
        /// </summary>
        public async Task<List<RequirementTraceDto>> GetTraceChainAsync(int requirementId)
        {
            return await GetListAsync<RequirementTraceDto>($"/api/requirementtraces/chain/{requirementId}");
        }

        /// <summary>
        /// Gets a requirement trace by ID.
        /// </summary>
        public async Task<RequirementTraceDto?> GetRequirementTraceByIdAsync(int id)
        {
            return await GetAsync<RequirementTraceDto>($"/api/requirementtraces/{id}");
        }

        /// <summary>
        /// Creates a new requirement trace.
        /// </summary>
        public async Task<RequirementTraceDto?> CreateRequirementTraceAsync(RequirementTraceDto trace)
        {
            return await PostAsync<RequirementTraceDto, RequirementTraceDto>("/api/requirementtraces", trace);
        }

        /// <summary>
        /// Deletes a requirement trace.
        /// </summary>
        public async Task<bool> DeleteRequirementTraceAsync(int id)
        {
            return await DeleteAsync($"/api/requirementtraces/{id}");
        }

        /// <summary>
        /// Validates the current trace relationships.
        /// </summary>
        public async Task<string> ValidateTracesAsync()
        {
            try
            {
                var response = await _http.GetAsync("/api/requirementtraces/validate");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return "Validation failed";
            }
            catch
            {
                return "Validation failed";
            }
        }

        /// <summary>
        /// Gets the traceability matrix for a document.
        /// </summary>
        public async Task<TraceabilityMatrixDto?> GetDocumentTraceabilityMatrixAsync(int documentId, string direction, bool uncoveredOnly = false)
        {
            return await GetAsync<TraceabilityMatrixDto>($"/api/documents/{documentId}/traceability?direction={direction}&uncoveredOnly={uncoveredOnly}");
        }
    }
}