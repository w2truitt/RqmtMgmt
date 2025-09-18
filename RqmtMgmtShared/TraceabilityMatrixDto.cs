using System.ComponentModel.DataAnnotations;

namespace RqmtMgmtShared
{
    /// <summary>
    /// Response DTO for document traceability matrix data.
    /// Contains all requirements in a document and their trace relationships.
    /// </summary>
    public class TraceabilityMatrixDto
    {
        /// <summary>
        /// Gets or sets the document ID for which traceability is requested.
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the document name/title.
        /// </summary>
        public string DocumentName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the document type (CRD, PRD, SRS).
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the trace direction (upstream or downstream).
        /// </summary>
        public string Direction { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the traceability data for requirements in this document.
        /// </summary>
        public List<RequirementTraceabilityDto> Traceability { get; set; } = new List<RequirementTraceabilityDto>();

        /// <summary>
        /// Gets or sets coverage statistics for this document.
        /// </summary>
        public CoverageStatsDto CoverageStats { get; set; } = new CoverageStatsDto();
    }

    /// <summary>
    /// Represents traceability information for a single requirement.
    /// </summary>
    public class RequirementTraceabilityDto
    {
        /// <summary>
        /// Gets or sets the source requirement (requirement in the current document).
        /// </summary>
        public RequirementSummaryDto SourceRequirement { get; set; } = new RequirementSummaryDto();

        /// <summary>
        /// Gets or sets the list of trace relationships from/to this requirement.
        /// </summary>
        public List<TraceRelationshipDto> Traces { get; set; } = new List<TraceRelationshipDto>();
    }

    /// <summary>
    /// Represents a single trace relationship.
    /// </summary>
    public class TraceRelationshipDto
    {
        /// <summary>
        /// Gets or sets the trace ID.
        /// </summary>
        public int TraceId { get; set; }

        /// <summary>
        /// Gets or sets the type of trace relationship.
        /// </summary>
        public TraceType TraceType { get; set; }

        /// <summary>
        /// Gets or sets the target requirement (requirement being traced to/from).
        /// </summary>
        public RequirementSummaryDto TargetRequirement { get; set; } = new RequirementSummaryDto();
    }

    /// <summary>
    /// Summary information for a requirement (lightweight version for traceability display).
    /// </summary>
    public class RequirementSummaryDto
    {
        /// <summary>
        /// Gets or sets the requirement ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the requirement title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full requirement ID (e.g., "PRD-001").
        /// </summary>
        public string FullRequirementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the requirement type.
        /// </summary>
        public RequirementType Type { get; set; }

        /// <summary>
        /// Gets or sets the document ID containing this requirement.
        /// </summary>
        public int? DocumentId { get; set; }

        /// <summary>
        /// Gets or sets the document name containing this requirement.
        /// </summary>
        public string DocumentName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Coverage statistics for traceability analysis.
    /// </summary>
    public class CoverageStatsDto
    {
        /// <summary>
        /// Gets or sets the total number of requirements in the document.
        /// </summary>
        public int TotalRequirements { get; set; }

        /// <summary>
        /// Gets or sets the number of requirements that have trace links.
        /// </summary>
        public int CoveredRequirements { get; set; }

        /// <summary>
        /// Gets or sets the coverage percentage (0-100).
        /// </summary>
        public decimal CoveragePercentage { get; set; }

        /// <summary>
        /// Gets or sets the list of requirements without trace links.
        /// </summary>
        public List<RequirementSummaryDto> UncoveredRequirements { get; set; } = new List<RequirementSummaryDto>();
    }
}