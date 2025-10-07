using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSectionHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add ProjectId column to DocumentSections (nullable initially to allow data population)
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "DocumentSections",
                type: "int",
                nullable: true);

            // Step 2: Populate ProjectId for existing sections
            // For root sections (DocumentId is not null), get ProjectId from Document
            migrationBuilder.Sql(@"
                UPDATE ds 
                SET ProjectId = d.ProjectId
                FROM DocumentSections ds
                INNER JOIN Documents d ON ds.DocumentId = d.Id
                WHERE ds.DocumentId IS NOT NULL
            ");

            // For subsections (DocumentId is null), get ProjectId from parent section hierarchy
            // This may need multiple iterations for deeply nested sections
            migrationBuilder.Sql(@"
                WITH SectionHierarchy AS (
                    -- Base case: sections that already have ProjectId
                    SELECT Id, ProjectId, ParentSectionId, 0 as Level
                    FROM DocumentSections
                    WHERE ProjectId IS NOT NULL
                    
                    UNION ALL
                    
                    -- Recursive case: get ProjectId from parent
                    SELECT ds.Id, sh.ProjectId, ds.ParentSectionId, sh.Level + 1
                    FROM DocumentSections ds
                    INNER JOIN SectionHierarchy sh ON ds.ParentSectionId = sh.Id
                    WHERE ds.ProjectId IS NULL AND sh.Level < 10
                )
                UPDATE ds
                SET ProjectId = sh.ProjectId
                FROM DocumentSections ds
                INNER JOIN SectionHierarchy sh ON ds.Id = sh.Id
                WHERE ds.ProjectId IS NULL
            ");

            // Step 3: For any remaining sections without ProjectId, set to Project 1 (default)
            migrationBuilder.Sql(@"
                UPDATE DocumentSections 
                SET ProjectId = 1 
                WHERE ProjectId IS NULL
            ");

            // Step 4: Make ProjectId non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "DocumentSections",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Step 5: Create a default "General" section for any requirements without SectionId
            migrationBuilder.Sql(@"
                -- Create General sections for documents that have requirements without SectionId
                INSERT INTO DocumentSections (ProjectId, DocumentId, ParentSectionId, Title, Description, SectionOrder, Level, SectionNumber, IsNotApplicable, CreatedAt)
                SELECT DISTINCT 
                    d.ProjectId,
                    r.DocumentId,
                    NULL,
                    'General Requirements',
                    'Default section for requirements not assigned to specific sections',
                    999,
                    1,
                    '999',
                    0,
                    GETUTCDATE()
                FROM Requirements r
                INNER JOIN Documents d ON r.DocumentId = d.Id
                WHERE r.SectionId IS NULL AND r.DocumentId IS NOT NULL
            ");

            // Step 6: Update requirements without SectionId to use the General section
            migrationBuilder.Sql(@"
                UPDATE r
                SET SectionId = ds.Id
                FROM Requirements r
                INNER JOIN Documents d ON r.DocumentId = d.Id
                INNER JOIN DocumentSections ds ON ds.DocumentId = d.Id 
                WHERE r.SectionId IS NULL 
                AND r.DocumentId IS NOT NULL
                AND ds.Title = 'General Requirements'
            ");

            // Step 7: For requirements without DocumentId, create a default document and section
            migrationBuilder.Sql(@"
                -- Create a default document for orphaned requirements
                IF NOT EXISTS (SELECT 1 FROM Documents WHERE Title = 'Orphaned Requirements Document')
                BEGIN
                    INSERT INTO Documents (Type, Title, Version, Status, CreatedBy, CreatedAt, ProjectId)
                    VALUES (2, 'Orphaned Requirements Document', '1.0', 0, 1, GETUTCDATE(), 1)
                END

                -- Create a section in the default document
                DECLARE @DefaultDocId int = (SELECT Id FROM Documents WHERE Title = 'Orphaned Requirements Document')
                
                IF NOT EXISTS (SELECT 1 FROM DocumentSections WHERE DocumentId = @DefaultDocId)
                BEGIN
                    INSERT INTO DocumentSections (ProjectId, DocumentId, ParentSectionId, Title, Description, SectionOrder, Level, SectionNumber, IsNotApplicable, CreatedAt)
                    VALUES (1, @DefaultDocId, NULL, 'General Requirements', 'Section for orphaned requirements', 1, 1, '1', 0, GETUTCDATE())
                END

                -- Update orphaned requirements
                UPDATE r
                SET DocumentId = @DefaultDocId,
                    SectionId = (SELECT Id FROM DocumentSections WHERE DocumentId = @DefaultDocId AND Title = 'General Requirements')
                FROM Requirements r
                WHERE r.SectionId IS NULL OR r.DocumentId IS NULL
            ");

            // Step 8: Make SectionId non-nullable in Requirements
            migrationBuilder.AlterColumn<int>(
                name: "SectionId",
                table: "Requirements",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Step 9: Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_DocumentSections_ProjectId",
                table: "DocumentSections",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSections_ProjectId_DocumentId",
                table: "DocumentSections",
                columns: new[] { "ProjectId", "DocumentId" });

            // Step 10: Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_DocumentSections_Projects_ProjectId",
                table: "DocumentSections",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Step 10.5: Clean up sections that have both DocumentId and ParentSectionId
            // Subsections should only reference their parent, not the document directly
            migrationBuilder.Sql(@"
                UPDATE DocumentSections
                SET DocumentId = NULL
                WHERE ParentSectionId IS NOT NULL AND DocumentId IS NOT NULL
            ");

            // Step 11: Add check constraint to ensure proper hierarchy
            migrationBuilder.AddCheckConstraint(
                name: "CK_DocumentSection_HierarchyIntegrity",
                table: "DocumentSections",
                sql: "([DocumentId] IS NOT NULL AND [ParentSectionId] IS NULL) OR ([DocumentId] IS NULL AND [ParentSectionId] IS NOT NULL)");

            // Step 12: Remove the old constraint that allowed both to be null
            migrationBuilder.DropCheckConstraint(
                name: "CK_DocumentSection_ParentReference",
                table: "DocumentSections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentSections_Projects_ProjectId",
                table: "DocumentSections");

            // Remove indexes
            migrationBuilder.DropIndex(
                name: "IX_DocumentSections_ProjectId",
                table: "DocumentSections");

            migrationBuilder.DropIndex(
                name: "IX_DocumentSections_ProjectId_DocumentId",
                table: "DocumentSections");

            // Remove check constraint
            migrationBuilder.DropCheckConstraint(
                name: "CK_DocumentSection_HierarchyIntegrity",
                table: "DocumentSections");

            // Make SectionId nullable again
            migrationBuilder.AlterColumn<int>(
                name: "SectionId",
                table: "Requirements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Remove ProjectId column
            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "DocumentSections");

            // Restore old constraint
            migrationBuilder.AddCheckConstraint(
                name: "CK_DocumentSection_ParentReference",
                table: "DocumentSections",
                sql: "([DocumentId] IS NOT NULL) OR ([ParentSectionId] IS NOT NULL)");
        }
    }
}