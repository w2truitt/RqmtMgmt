using RqmtMgmtShared;
using System.Net.Http.Json;

namespace frontend.Services
{
    /// <summary>
    /// Service for managing requirement traces through the API.
    /// </summary>
    public class RequirementTracesDataService : BaseDataService, IRequirementTraceService
    {
        public RequirementTracesDataService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Gets outgoing traces from a source requirement.
        /// </summary>
        public async Task<List<RequirementTraceDto>> GetBySourceRequirementIdAsync(int sourceRequirementId)
        {
            return await GetListAsync<RequirementTraceDto>($"/api/requirementtraces/source/{sourceRequirementId}");
        }

        /// <summary>
        /// Gets incoming traces to a target requirement.
        /// </summary>
        public async Task<List<RequirementTraceDto>> GetByTargetRequirementIdAsync(int targetRequirementId)
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
        public async Task<RequirementTraceDto?> GetByIdAsync(int id)
        {
            return await GetAsync<RequirementTraceDto>($"/api/requirementtraces/{id}");
        }

        /// <summary>
        /// Creates a new requirement trace.
        /// </summary>
        public async Task<RequirementTraceDto?> CreateAsync(RequirementTraceDto trace)
        {
            return await PostAsync<RequirementTraceDto, RequirementTraceDto>("/api/requirementtraces", trace);
        }

        /// <summary>
        /// Deletes a requirement trace.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            return await DeleteAsync($"/api/requirementtraces/{id}");
        }

        /// <summary>
        /// Validates a trace relationship.
        /// </summary>
        public async Task<bool> ValidateTraceAsync(int sourceRequirementId, int targetRequirementId, TraceType traceType)
        {
            try
            {
                var response = await _http.PostAsync($"/api/requirementtraces/validate?sourceId={sourceRequirementId}&targetId={targetRequirementId}&traceType={traceType}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Additional methods for the TraceabilityMatrix component (not part of interface)
        /// <summary>
        /// Gets the traceability matrix for a document.
        /// </summary>
        public async Task<TraceabilityMatrixDto?> GetDocumentTraceabilityMatrixAsync(int documentId, string direction, bool uncoveredOnly = false)
        {
            return await GetAsync<TraceabilityMatrixDto>($"/api/documents/{documentId}/traceability?direction={direction}&uncoveredOnly={uncoveredOnly}");
        }

        /// <summary>
        /// Gets the traceability matrix for a document (alias for GetDocumentTraceabilityMatrixAsync).
        /// </summary>
        public async Task<TraceabilityMatrixDto?> GetDocumentTraceabilityAsync(int documentId, string direction, bool uncoveredOnly = false)
        {
            return await GetDocumentTraceabilityMatrixAsync(documentId, direction, uncoveredOnly);
        }

        // Legacy methods for backward compatibility
        public async Task<List<RequirementTraceDto>> GetSourceTracesAsync(int sourceRequirementId) => await GetBySourceRequirementIdAsync(sourceRequirementId);
        public async Task<List<RequirementTraceDto>> GetTargetTracesAsync(int targetRequirementId) => await GetByTargetRequirementIdAsync(targetRequirementId);
        public async Task<RequirementTraceDto?> GetRequirementTraceByIdAsync(int id) => await GetByIdAsync(id);
        public async Task<RequirementTraceDto?> CreateRequirementTraceAsync(RequirementTraceDto trace) => await CreateAsync(trace);
        public async Task<bool> DeleteRequirementTraceAsync(int id) => await DeleteAsync(id);

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
    }
}