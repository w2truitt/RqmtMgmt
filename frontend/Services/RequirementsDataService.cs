using System.Net.Http.Json;
using System.Text.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for requirements management operations.
    /// Provides HTTP client-based implementation of IRequirementService for communicating with the backend API.
    /// </summary>
    public class RequirementsDataService : IRequirementService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the RequirementsDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public RequirementsDataService(HttpClient http)
        {
            _http = http;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
        }

        /// <summary>
        /// Retrieves all requirements from the backend API.
        /// </summary>
        /// <returns>A list of all requirements, or an empty list if the request fails.</returns>
        public async Task<List<RequirementDto>> GetAllAsync()
            => await _http.GetFromJsonAsync<List<RequirementDto>>("/api/Requirement", _jsonOptions) ?? new();

        /// <summary>
        /// Retrieves a paginated list of requirements from the backend API.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, search term, and sorting options.</param>
        /// <returns>A paginated result containing requirements and pagination metadata.</returns>
        public async Task<PagedResult<RequirementDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var queryString = $"?pageNumber={parameters.PageNumber}&pageSize={parameters.PageSize}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(parameters.SearchTerm)}";
            
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
                queryString += $"&sortBy={Uri.EscapeDataString(parameters.SortBy)}";
            
            if (parameters.SortDescending)
                queryString += "&sortDescending=true";

            // Include ProjectId in the query string if provided
            if (parameters.ProjectId.HasValue)
                queryString += $"&projectId={parameters.ProjectId.Value}";

            var result = await _http.GetFromJsonAsync<PagedResult<RequirementDto>>($"/api/Requirement/paged{queryString}", _jsonOptions);
            return result ?? new PagedResult<RequirementDto>();
        }

        /// <summary>
        /// Retrieves a specific requirement by its ID from the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement.</param>
        /// <returns>The requirement if found; otherwise, null.</returns>
        public async Task<RequirementDto?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<RequirementDto>($"/api/Requirement/{id}", _jsonOptions);

        /// <summary>
        /// Creates a new requirement by sending a POST request to the backend API.
        /// </summary>
        /// <param name="dto">The requirement data to create.</param>
        /// <returns>The created requirement with its assigned ID if successful; otherwise, null.</returns>
        public async Task<RequirementDto?> CreateAsync(RequirementDto dto)
        {
            var resp = await _http.PostAsJsonAsync("/api/Requirement", dto, _jsonOptions);
            return await resp.Content.ReadFromJsonAsync<RequirementDto>(_jsonOptions);
        }

        /// <summary>
        /// Updates an existing requirement by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="dto">The requirement data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(RequirementDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"/api/Requirement/{dto.Id}", dto, _jsonOptions);
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// Deletes a requirement by sending a DELETE request to the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/Requirement/{id}");
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves the version history for a specific requirement from the backend API.
        /// </summary>
        /// <param name="requirementId">The unique identifier of the requirement.</param>
        /// <returns>A list of requirement versions, or an empty list if the request fails.</returns>
        public async Task<List<RequirementVersionDto>> GetVersionsAsync(int requirementId)
            => await _http.GetFromJsonAsync<List<RequirementVersionDto>>($"/api/Redline/requirement/{requirementId}/versions", _jsonOptions) ?? new();
    }
}