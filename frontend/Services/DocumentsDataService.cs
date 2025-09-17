using RqmtMgmtShared;
using System.Net.Http.Json;

namespace frontend.Services
{
    /// <summary>
    /// Service for managing documents (CRD, PRD, SRS) through the API.
    /// </summary>
    public class DocumentsDataService : BaseDataService
    {
        public DocumentsDataService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Gets all documents.
        /// </summary>
        public async Task<List<DocumentDto>> GetDocumentsAsync()
        {
            return await GetListAsync<DocumentDto>("/api/documents");
        }

        /// <summary>
        /// Gets documents with pagination.
        /// </summary>
        public async Task<PagedResult<DocumentDto>> GetDocumentsPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
        {
            var queryParams = new Dictionary<string, object?>
            {
                ["pageNumber"] = pageNumber > 1 ? pageNumber : null,
                ["pageSize"] = pageSize != 10 ? pageSize : null,
                ["searchTerm"] = searchTerm,
                ["sortBy"] = sortBy,
                ["sortDescending"] = sortDescending ? "true" : null
            };

            var queryString = BuildQueryString(queryParams);
            
            return await GetAsync<PagedResult<DocumentDto>>($"/api/documents/paged{queryString}") ?? new PagedResult<DocumentDto>
            {
                Items = new List<DocumentDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = 0
            };
        }

        /// <summary>
        /// Gets documents by project ID.
        /// </summary>
        public async Task<List<DocumentDto>> GetDocumentsByProjectAsync(int projectId)
        {
            return await GetListAsync<DocumentDto>($"/api/documents/project/{projectId}");
        }

        /// <summary>
        /// Gets documents by type.
        /// </summary>
        public async Task<List<DocumentDto>> GetDocumentsByTypeAsync(DocumentType type)
        {
            return await GetListAsync<DocumentDto>($"/api/documents/type/{type}");
        }

        /// <summary>
        /// Gets a document by ID.
        /// </summary>
        public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
        {
            return await GetAsync<DocumentDto>($"/api/documents/{id}");
        }

        /// <summary>
        /// Creates a new document.
        /// </summary>
        public async Task<DocumentDto?> CreateDocumentAsync(DocumentDto document)
        {
            return await PostAsync<DocumentDto, DocumentDto>("/api/documents", document);
        }

        /// <summary>
        /// Updates an existing document.
        /// </summary>
        public async Task<bool> UpdateDocumentAsync(DocumentDto document)
        {
            return await PutAsync($"/api/documents/{document.Id}", document);
        }

        /// <summary>
        /// Deletes a document.
        /// </summary>
        public async Task<bool> DeleteDocumentAsync(int id)
        {
            return await DeleteAsync($"/api/documents/{id}");
        }
    }
}