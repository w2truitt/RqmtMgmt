        /// <summary>
        /// Retrieves traceability matrix data for a document showing requirement trace relationships.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <param name="direction">The trace direction: 'upstream' or 'downstream'.</param>
        /// <param name="uncoveredOnly">If true, returns only requirements without trace links.</param>
        /// <returns>Traceability matrix data for the document.</returns>
        public async Task<TraceabilityMatrixDto?> GetTraceabilityMatrixAsync(int documentId, string direction, bool uncoveredOnly = false)
        {
            // First, get the document to ensure it exists
            var document = await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return null;

            // Get all requirements for this document
            var documentRequirements = await _context.Requirements
                .Where(r => r.DocumentId == documentId)
                .AsNoTracking()
                .ToListAsync();

            if (!documentRequirements.Any())
            {
                return new TraceabilityMatrixDto
                {
                    DocumentId = documentId,
                    DocumentName = document.Title,
                    DocumentType = document.Type,
                    Direction = direction,
                    Traceability = new List<RequirementTraceabilityDto>(),
                    CoverageStats = new CoverageStatsDto
                    {
                        TotalRequirements = 0,
                        CoveredRequirements = 0,
                        CoveragePercentage = 0,
                        UncoveredRequirements = new List<RequirementSummaryDto>()
                    }
                };
            }

            var requirementIds = documentRequirements.Select(r => r.Id).ToList();

            // Get trace relationships based on direction
            List<RequirementTrace> traces;
            if (direction == "downstream")
            {
                // Get traces FROM requirements in this document TO other requirements
                traces = await _context.RequirementTraces
                    .Include(rt => rt.SourceRequirement)
                    .Include(rt => rt.TargetRequirement)
                    .Where(rt => requirementIds.Contains(rt.SourceRequirementId))
                    .AsNoTracking()
                    .ToListAsync();
            }
            else // upstream
            {
                // Get traces FROM other requirements TO requirements in this document
                traces = await _context.RequirementTraces
                    .Include(rt => rt.SourceRequirement)
                    .Include(rt => rt.TargetRequirement)
                    .Where(rt => requirementIds.Contains(rt.TargetRequirementId))
                    .AsNoTracking()
                    .ToListAsync();
            }

            // Build traceability data
            var traceabilityList = new List<RequirementTraceabilityDto>();
            var coveredRequirementIds = new HashSet<int>();

            foreach (var requirement in documentRequirements)
            {
                var relevantTraces = direction == "downstream" 
                    ? traces.Where(t => t.SourceRequirementId == requirement.Id).ToList()
                    : traces.Where(t => t.TargetRequirementId == requirement.Id).ToList();

                if (relevantTraces.Any())
                    coveredRequirementIds.Add(requirement.Id);

                // Skip uncovered requirements if uncoveredOnly is false, or skip covered if uncoveredOnly is true
                if (uncoveredOnly && relevantTraces.Any()) continue;
                if (!uncoveredOnly || !relevantTraces.Any())
                {
                    var traceRelationships = relevantTraces.Select(trace => new TraceRelationshipDto
                    {
                        TraceId = trace.Id,
                        TraceType = trace.TraceType,
                        TargetRequirement = new RequirementSummaryDto
                        {
                            Id = direction == "downstream" ? trace.TargetRequirement!.Id : trace.SourceRequirement!.Id,
                            Title = direction == "downstream" ? trace.TargetRequirement!.Title : trace.SourceRequirement!.Title,
                            FullRequirementId = GenerateFullRequirementId(direction == "downstream" ? trace.TargetRequirement! : trace.SourceRequirement!),
                            Type = direction == "downstream" ? trace.TargetRequirement!.Type : trace.SourceRequirement!.Type,
                            DocumentId = direction == "downstream" ? trace.TargetRequirement!.DocumentId : trace.SourceRequirement!.DocumentId,
                            DocumentName = GetDocumentName(direction == "downstream" ? trace.TargetRequirement!.DocumentId : trace.SourceRequirement!.DocumentId)
                        }
                    }).ToList();

                    traceabilityList.Add(new RequirementTraceabilityDto
                    {
                        SourceRequirement = new RequirementSummaryDto
                        {
                            Id = requirement.Id,
                            Title = requirement.Title,
                            FullRequirementId = GenerateFullRequirementId(requirement),
                            Type = requirement.Type,
                            DocumentId = requirement.DocumentId,
                            DocumentName = document.Title
                        },
                        Traces = traceRelationships
                    });
                }
            }

            // Calculate coverage statistics
            var uncoveredRequirements = documentRequirements
                .Where(r => !coveredRequirementIds.Contains(r.Id))
                .Select(r => new RequirementSummaryDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    FullRequirementId = GenerateFullRequirementId(r),
                    Type = r.Type,
                    DocumentId = r.DocumentId,
                    DocumentName = document.Title
                }).ToList();

            var coverageStats = new CoverageStatsDto
            {
                TotalRequirements = documentRequirements.Count,
                CoveredRequirements = coveredRequirementIds.Count,
                CoveragePercentage = documentRequirements.Count > 0 
                    ? Math.Round((decimal)coveredRequirementIds.Count / documentRequirements.Count * 100, 1) 
                    : 0,
                UncoveredRequirements = uncoveredRequirements
            };

            return new TraceabilityMatrixDto
            {
                DocumentId = documentId,
                DocumentName = document.Title,
                DocumentType = document.Type,
                Direction = direction,
                Traceability = traceabilityList,
                CoverageStats = coverageStats
            };
        }

        /// <summary>
        /// Generates the full requirement ID (e.g., "PRD-001") for a requirement.
        /// </summary>
        private static string GenerateFullRequirementId(Requirement requirement)
        {
            // This is a simplified version - you may want to implement more sophisticated logic
            var prefix = requirement.Type.ToString().ToUpper();
            return $"{prefix}-{requirement.Id:D3}";
        }

        /// <summary>
        /// Gets the document name for a given document ID.
        /// </summary>
        private string GetDocumentName(int? documentId)
        {
            if (!documentId.HasValue) return string.Empty;
            
            var doc = _context.Documents
                .AsNoTracking()
                .FirstOrDefault(d => d.Id == documentId.Value);
            
            return doc?.Title ?? string.Empty;
        }