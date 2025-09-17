using System.Net.Http.Json;
using System.Text.Json;
using RqmtMgmtShared;

namespace frontend.Services
{
    /// <summary>
    /// Frontend data service for requirements management operations.
    /// Provides HTTP client-based implementation of IRequirementService for communicating with the backend API.
    /// Now inherits from BaseDataService to eliminate code duplication and ensure consistency.
    /// </summary>
    public class RequirementsDataService : BaseDataService, IRequirementService
    {
        /// <summary>
        /// Initializes a new instance of the RequirementsDataService with the specified HTTP client.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        public RequirementsDataService(HttpClient http) : base(http)
        {
        }

        /// <summary>
        /// Retrieves all requirements from the backend API.
        /// </summary>
        /// <returns>A list of all requirements, or an empty list if the request fails.</returns>
        public async Task<List<RequirementDto>> GetAllAsync()
            => await GetListAsync<RequirementDto>("/api/Requirement");

        /// <summary>
        /// Retrieves all requirements for a specific project from the backend API.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of requirements for the specified project, or an empty list if the request fails.</returns>
        public async Task<List<RequirementDto>> GetByProjectIdAsync(int projectId)
            => await GetListAsync<RequirementDto>($"/api/Requirement/project/{projectId}");

        /// <summary>
        /// Retrieves a specific requirement by its ID from the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement.</param>
        /// <returns>The requirement if found; otherwise, null.</returns>
        public async Task<RequirementDto?> GetByIdAsync(int id)
            => await GetAsync<RequirementDto>($"/api/Requirement/{id}");

        /// <summary>
        /// Creates a new requirement by sending a POST request to the backend API.
        /// </summary>
        /// <param name="requirement">The requirement data to create.</param>
        /// <returns>The created requirement with its assigned ID if successful; otherwise, null.</returns>
        public async Task<RequirementDto?> CreateAsync(RequirementDto requirement)
            => await PostAsync<RequirementDto, RequirementDto>("/api/Requirement", requirement);

        /// <summary>
        /// Updates an existing requirement by sending a PUT request to the backend API.
        /// </summary>
        /// <param name="requirement">The requirement data to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateAsync(RequirementDto requirement)
            => await PutAsync($"/api/Requirement/{requirement.Id}", requirement);

        /// <summary>
        /// Deletes a requirement by sending a DELETE request to the backend API.
        /// </summary>
        /// <param name="id">The unique identifier of the requirement to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteAsync(int id)
            => await DeleteAsync($"/api/Requirement/{id}");

        /// <summary>
        /// Retrieves requirements with pagination from the backend API.
        /// </summary>
        /// <param name="parameters">Pagination parameters including page number, size, and search criteria.</param>
        /// <returns>A paginated result containing requirements and pagination metadata.</returns>
        public async Task<PagedResult<RequirementDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["PageNumber"] = parameters.PageNumber,
                ["PageSize"] = parameters.PageSize,
                ["SearchTerm"] = parameters.SearchTerm,
                ["SortBy"] = parameters.SortBy,
                ["SortDescending"] = parameters.SortDescending ? "true" : null,
                ["ProjectId"] = parameters.ProjectId
            };

            var queryString = BuildQueryString(queryParams);
            var result = await GetAsync<PagedResult<RequirementDto>>($"/api/Requirement/paged{queryString}");
            return result ?? new PagedResult<RequirementDto>();
        }

        /// <summary>
        /// Retrieves requirements for a specific project with pagination from the backend API.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="parameters">Pagination parameters including page number, size, and search criteria.</param>
        /// <returns>A paginated result containing requirements for the project and pagination metadata.</returns>
        public async Task<PagedResult<RequirementDto>> GetPagedByProjectIdAsync(int projectId, PaginationParameters parameters)
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["PageNumber"] = parameters.PageNumber,
                ["PageSize"] = parameters.PageSize,
                ["SearchTerm"] = parameters.SearchTerm,
                ["SortBy"] = parameters.SortBy,
                ["SortDescending"] = parameters.SortDescending ? "true" : null
            };

            var queryString = BuildQueryString(queryParams);
            var result = await GetAsync<PagedResult<RequirementDto>>($"/api/Requirement/project/{projectId}/paged{queryString}");
            return result ?? new PagedResult<RequirementDto>();
        }

        /// <summary>
        /// Retrieves all versions of a specific requirement from the backend API.
        /// </summary>
        /// <param name="requirementId">The unique identifier of the requirement.</param>
        /// <returns>A list of requirement versions, or an empty list if the request fails.</returns>
        public async Task<List<RequirementVersionDto>> GetVersionsAsync(int requirementId)
            => await GetListAsync<RequirementVersionDto>($"/api/Requirement/{requirementId}/versions");

        /// <summary>
        /// Retrieves all requirements for a specific document from the backend API.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document.</param>
        /// <returns>A list of requirements for the specified document, or an empty list if the request fails.</returns>
        public async Task<List<RequirementDto>> GetByDocumentIdAsync(int documentId)
            => await GetListAsync<RequirementDto>($"/api/Requirement/document/{documentId}");

        /// <summary>
        /// Retrieves requirements for a specific document with pagination from the backend API.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document.</param>
        /// <param name="parameters">Pagination parameters including page number, size, and search criteria.</param>
        /// <returns>A paginated result containing requirements for the document and pagination metadata.</returns>
        public async Task<PagedResult<RequirementDto>> GetPagedByDocumentIdAsync(int documentId, PaginationParameters parameters)
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["PageNumber"] = parameters.PageNumber,
                ["PageSize"] = parameters.PageSize,
                ["SearchTerm"] = parameters.SearchTerm,
                ["SortBy"] = parameters.SortBy,
                ["SortDescending"] = parameters.SortDescending ? "true" : null
            };

            var queryString = BuildQueryString(queryParams);
            var result = await GetAsync<PagedResult<RequirementDto>>($"/api/Requirement/document/{documentId}/paged{queryString}");
            return result ?? new PagedResult<RequirementDto>();
        }

        /// <summary>
        /// Retrieves all requirements for a specific document section from the backend API.
        /// </summary>
        /// <param name="sectionId">The unique identifier of the document section.</param>
        /// <returns>A list of requirements for the specified section, or an empty list if the request fails.</returns>
        public async Task<List<RequirementDto>> GetBySectionIdAsync(int sectionId)
            => await GetListAsync<RequirementDto>($"/api/Requirement/section/{sectionId}");

        /// <summary>
        /// Retrieves requirements for a specific document section with pagination from the backend API.
        /// </summary>
        /// <param name="sectionId">The unique identifier of the document section.</param>
        /// <param name="parameters">Pagination parameters including page number, size, and search criteria.</param>
        /// <returns>A paginated result containing requirements for the section and pagination metadata.</returns>
        public async Task<PagedResult<RequirementDto>> GetPagedBySectionIdAsync(int sectionId, PaginationParameters parameters)
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["PageNumber"] = parameters.PageNumber,
                ["PageSize"] = parameters.PageSize,
                ["SearchTerm"] = parameters.SearchTerm,
                ["SortBy"] = parameters.SortBy,
                ["SortDescending"] = parameters.SortDescending ? "true" : null
            };

            var queryString = BuildQueryString(queryParams);
            var result = await GetAsync<PagedResult<RequirementDto>>($"/api/Requirement/section/{sectionId}/paged{queryString}");
            return result ?? new PagedResult<RequirementDto>();
        }
    }
}