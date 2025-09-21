using RqmtMgmtShared;
using System.Net.Http.Json;

namespace frontend.Services
{
    /// <summary>
    /// Service for managing document sections through the API.
    /// </summary>
    public class DocumentSectionsDataService : BaseDataService, IDocumentSectionService
    {
        public DocumentSectionsDataService(HttpClient httpClient) : base(httpClient)
        {
        }

        /// <summary>
        /// Gets all sections for a document.
        /// </summary>
        public async Task<List<DocumentSectionDto>> GetByDocumentIdAsync(int documentId)
        {
            return await GetListAsync<DocumentSectionDto>($"/api/documentsections/document/{documentId}");
        }

        /// <summary>
        /// Gets a document section by ID.
        /// </summary>
        public async Task<DocumentSectionDto?> GetByIdAsync(int id)
        {
            return await GetAsync<DocumentSectionDto>($"/api/documentsections/{id}");
        }

        /// <summary>
        /// Creates a new document section.
        /// </summary>
        public async Task<DocumentSectionDto?> CreateAsync(DocumentSectionDto section)
        {
            return await PostAsync<DocumentSectionDto, DocumentSectionDto>("/api/documentsections", section);
        }

        /// <summary>
        /// Updates an existing document section.
        /// </summary>
        public async Task<bool> UpdateAsync(DocumentSectionDto section)
        {
            return await PutAsync($"/api/documentsections/{section.Id}", section);
        }

        /// <summary>
        /// Deletes a document section.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            return await DeleteAsync($"/api/documentsections/{id}");
        }

        /// <summary>
        /// Reorders document sections.
        /// </summary>
        public async Task<bool> ReorderSectionsAsync(int documentId, List<int> sectionIds)
        {
            var reorderRequests = sectionIds.Select((sectionId, index) => new ReorderSectionRequest
            {
                SectionId = sectionId,
                SectionOrder = index + 1
            }).ToList();

            return await PostAsync($"/api/documentsections/document/{documentId}/reorder", reorderRequests);
        }

        // Legacy methods for backward compatibility
        public async Task<List<DocumentSectionDto>> GetDocumentSectionsAsync(int documentId) => await GetByDocumentIdAsync(documentId);
        public async Task<DocumentSectionDto?> GetDocumentSectionByIdAsync(int id) => await GetByIdAsync(id);
        public async Task<DocumentSectionDto?> CreateDocumentSectionAsync(DocumentSectionDto section) => await CreateAsync(section);
        public async Task<bool> UpdateDocumentSectionAsync(DocumentSectionDto section) => await UpdateAsync(section);
        public async Task<bool> DeleteDocumentSectionAsync(int id) => await DeleteAsync(id);
        public async Task<bool> ReorderDocumentSectionsAsync(int documentId, List<ReorderSectionRequest> reorderRequests)
        {
            return await PostAsync($"/api/documentsections/document/{documentId}/reorder", reorderRequests);
        }
    }

    /// <summary>
    /// Request object for reordering sections.
    /// </summary>
    public class ReorderSectionRequest
    {
        public int SectionId { get; set; }
        public int SectionOrder { get; set; }
    }
}