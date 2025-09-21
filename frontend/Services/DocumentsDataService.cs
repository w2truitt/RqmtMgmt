using RqmtMgmtShared;
using System.Net.Http.Json;

namespace frontend.Services
{
    /// <summary>
    /// Service for managing documents (CRD, PRD, SRS) through the API.
    /// </summary>
    public class DocumentsDataService : BaseDataService, IDocumentService
    {
        public DocumentsDataService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Gets all documents.
        /// </summary>
        public async Task<List<DocumentDto>> GetAllAsync()
        {
            return await GetListAsync<DocumentDto>("/api/documents");
        }

        /// <summary>
        /// Gets documents with pagination.
        /// </summary>
        public async Task<PagedResult<DocumentDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["pageNumber"] = parameters.PageNumber > 1 ? parameters.PageNumber : null,
                ["pageSize"] = parameters.PageSize != 10 ? parameters.PageSize : null,
                ["searchTerm"] = parameters.SearchTerm,
                ["sortBy"] = parameters.SortBy,
                ["sortDescending"] = parameters.SortDescending ? "true" : null
            };

            var queryString = BuildQueryString(queryParams);
            
            return await GetAsync<PagedResult<DocumentDto>>($"/api/documents/paged{queryString}") ?? new PagedResult<DocumentDto>
            {
                Items = new List<DocumentDto>(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = 0
            };
        }

        /// <summary>
        /// Gets documents by project ID.
        /// </summary>
        public async Task<List<DocumentDto>> GetByProjectIdAsync(int projectId)
        {
            return await GetListAsync<DocumentDto>($"/api/documents/project/{projectId}");
        }

        /// <summary>
        /// Gets documents by project ID with pagination.
        /// </summary>
        public async Task<PagedResult<DocumentDto>> GetPagedByProjectIdAsync(int projectId, PaginationParameters parameters)
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["pageNumber"] = parameters.PageNumber > 1 ? parameters.PageNumber : null,
                ["pageSize"] = parameters.PageSize != 10 ? parameters.PageSize : null,
                ["searchTerm"] = parameters.SearchTerm,
                ["sortBy"] = parameters.SortBy,
                ["sortDescending"] = parameters.SortDescending ? "true" : null
            };

            var queryString = BuildQueryString(queryParams);
            
            return await GetAsync<PagedResult<DocumentDto>>($"/api/documents/project/{projectId}/paged{queryString}") ?? new PagedResult<DocumentDto>
            {
                Items = new List<DocumentDto>(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = 0
            };
        }

        /// <summary>
        /// Gets documents by type.
        /// </summary>
        public async Task<List<DocumentDto>> GetByTypeAsync(DocumentType type)
        {
            return await GetListAsync<DocumentDto>($"/api/documents/type/{type}");
        }

        /// <summary>
        /// Gets a document by ID.
        /// </summary>
        public async Task<DocumentDto?> GetByIdAsync(int id)
        {
            return await GetAsync<DocumentDto>($"/api/documents/{id}");
        }

        /// <summary>
        /// Creates a new document.
        /// </summary>
        public async Task<DocumentDto?> CreateAsync(DocumentDto document)
        {
            return await PostAsync<DocumentDto, DocumentDto>("/api/documents", document);
        }

        /// <summary>
        /// Updates an existing document.
        /// </summary>
        public async Task<bool> UpdateAsync(DocumentDto document)
        {
            return await PutAsync($"/api/documents/{document.Id}", document);
        }

        /// <summary>
        /// Deletes a document.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            return await DeleteAsync($"/api/documents/{id}");
        }

        /// <summary>
        /// Gets the traceability matrix for a document.
        /// </summary>
        public async Task<TraceabilityMatrixDto?> GetTraceabilityMatrixAsync(int documentId, string direction, bool uncoveredOnly = false)
        {
            return await GetAsync<TraceabilityMatrixDto>($"/api/documents/{documentId}/traceability?direction={direction}&uncoveredOnly={uncoveredOnly}");
        }

        // Legacy methods for backward compatibility
        public async Task<List<DocumentDto>> GetDocumentsAsync() => await GetAllAsync();
        public async Task<List<DocumentDto>> GetDocumentsByProjectAsync(int projectId) => await GetByProjectIdAsync(projectId);
        public async Task<DocumentDto?> GetDocumentByIdAsync(int id) => await GetByIdAsync(id);
        public async Task<DocumentDto?> CreateDocumentAsync(DocumentDto document) => await CreateAsync(document);
        public async Task<bool> UpdateDocumentAsync(DocumentDto document) => await UpdateAsync(document);
        public async Task<bool> DeleteDocumentAsync(int id) => await DeleteAsync(id);
    }
}