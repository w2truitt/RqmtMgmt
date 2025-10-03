using RqmtMgmtShared;
using System.Net.Http.Json;

namespace frontend.Services
{
    /// <summary>
    /// Service for managing document sections through the API with hierarchical support.
    /// </summary>
    public class DocumentSectionsDataService : BaseDataService, IDocumentSectionService
    {
        public DocumentSectionsDataService(HttpClient httpClient) : base(httpClient)
        {
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Gets all sections for a document (flat list).
        /// </summary>
        public async Task<List<DocumentSectionDto>> GetByDocumentIdAsync(int documentId)
        {
            return await GetListAsync<DocumentSectionDto>($"/api/documentsections/document/{documentId}?includeChildren=false");
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

        #endregion

        #region Hierarchical Query Operations

        /// <summary>
        /// Gets the full section hierarchy for a document.
        /// </summary>
        public async Task<List<DocumentSectionDto>> GetSectionHierarchyAsync(int documentId)
        {
            return await GetListAsync<DocumentSectionDto>($"/api/documentsections/document/{documentId}/hierarchy");
        }

        /// <summary>
        /// Gets a section with its children to a specified depth.
        /// </summary>
        public async Task<DocumentSectionDto?> GetSectionWithChildrenAsync(int sectionId, int depth = -1)
        {
            return await GetAsync<DocumentSectionDto>($"/api/documentsections/{sectionId}?includeChildren=true&depth={depth}");
        }

        /// <summary>
        /// Gets direct child sections of a parent section.
        /// </summary>
        public async Task<List<DocumentSectionDto>> GetChildSectionsAsync(int parentSectionId)
        {
            return await GetListAsync<DocumentSectionDto>($"/api/documentsections/{parentSectionId}/children");
        }

        /// <summary>
        /// Gets root-level sections for a document.
        /// </summary>
        public async Task<List<DocumentSectionDto>> GetRootSectionsAsync(int documentId)
        {
            return await GetListAsync<DocumentSectionDto>($"/api/documentsections/document/{documentId}/roots");
        }

        #endregion

        #region Hierarchy Management Operations

        /// <summary>
        /// Moves a section to a new parent.
        /// </summary>
        public async Task<bool> MoveSectionAsync(int sectionId, int? newParentId, int? newOrder = null)
        {
            var request = new { NewParentId = newParentId, NewOrder = newOrder };
            return await PostAsync($"/api/documentsections/{sectionId}/move", request);
        }

        /// <summary>
        /// Reorders sections within a parent.
        /// </summary>
        public async Task<bool> ReorderSectionsAsync(int? parentId, List<int> sectionIds)
        {
            var request = new { ParentId = parentId, SectionIds = sectionIds };
            return await PostAsync("/api/documentsections/reorder", request);
        }

        /// <summary>
        /// Validates if a parent reference is valid.
        /// </summary>
        public async Task<bool> ValidateParentReferenceAsync(int sectionId, int? parentId)
        {
            if (!parentId.HasValue)
                return true;
                
            try
            {
                var response = await _http.GetAsync($"/api/documentsections/{sectionId}/validate-parent/{parentId.Value}");
                if (!response.IsSuccessStatusCode)
                    return false;
                    
                var result = await response.Content.ReadFromJsonAsync<bool>();
                return result;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Utility Operations

        /// <summary>
        /// Generates a section number for a section.
        /// </summary>
        public async Task<string?> GenerateSectionNumberAsync(int sectionId)
        {
            return await GetAsync<string>($"/api/documentsections/{sectionId}/generate-number");
        }

        /// <summary>
        /// Gets the requirement count for a section.
        /// </summary>
        public async Task<int> GetRequirementCountAsync(int sectionId, bool recursive = false)
        {
            try
            {
                var response = await _http.GetAsync($"/api/documentsections/{sectionId}/requirement-count?recursive={recursive}");
                if (!response.IsSuccessStatusCode)
                    return 0;
                    
                var result = await response.Content.ReadFromJsonAsync<int>();
                return result;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Legacy Methods for Backward Compatibility

        public async Task<List<DocumentSectionDto>> GetDocumentSectionsAsync(int documentId) => await GetByDocumentIdAsync(documentId);
        public async Task<DocumentSectionDto?> GetDocumentSectionByIdAsync(int id) => await GetByIdAsync(id);
        public async Task<DocumentSectionDto?> CreateDocumentSectionAsync(DocumentSectionDto section) => await CreateAsync(section);
        public async Task<bool> UpdateDocumentSectionAsync(DocumentSectionDto section) => await UpdateAsync(section);
        public async Task<bool> DeleteDocumentSectionAsync(int id) => await DeleteAsync(id);
        
        public async Task<bool> ReorderDocumentSectionsAsync(int documentId, List<ReorderSectionRequest> reorderRequests)
        {
            // Convert legacy format to new format
            var sectionIds = reorderRequests.OrderBy(r => r.SectionOrder).Select(r => r.SectionId).ToList();
            return await ReorderSectionsAsync(null, sectionIds);
        }

        #endregion
    }

    /// <summary>
    /// Request object for reordering sections (legacy).
    /// </summary>
    public class ReorderSectionRequest
    {
        public int SectionId { get; set; }
        public int SectionOrder { get; set; }
    }
}