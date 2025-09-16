using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class DocumentCentricRefactorFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestCaseExecutions_Users_ExecutedBy",
                table: "TestCaseExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCases_TestSuites_SuiteId",
                table: "TestCases");

            migrationBuilder.DropForeignKey(
                name: "FK_TestRuns_TestPlans_TestPlanId",
                table: "TestRuns");

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Requirements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "Requirements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Objective = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Background = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InScope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutOfScope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserPersonas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuccessCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dependencies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssumptionsConstraints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Appendix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequirementTraces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceRequirementId = table.Column<int>(type: "int", nullable: false),
                    TargetRequirementId = table.Column<int>(type: "int", nullable: false),
                    TraceType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementTraces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequirementTraces_Requirements_SourceRequirementId",
                        column: x => x.SourceRequirementId,
                        principalTable: "Requirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequirementTraces_Requirements_TargetRequirementId",
                        column: x => x.TargetRequirementId,
                        principalTable: "Requirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequirementTraces_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionOrder = table.Column<int>(type: "int", nullable: false),
                    IsNotApplicable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentSections_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_DocumentId",
                table: "Requirements",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_SectionId",
                table: "Requirements",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CreatedBy",
                table: "Documents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ProjectId_Type",
                table: "Documents",
                columns: new[] { "ProjectId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSections_DocumentId_SectionOrder",
                table: "DocumentSections",
                columns: new[] { "DocumentId", "SectionOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RequirementTraces_CreatedBy",
                table: "RequirementTraces",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementTraces_SourceRequirementId_TraceType",
                table: "RequirementTraces",
                columns: new[] { "SourceRequirementId", "TraceType" });

            migrationBuilder.CreateIndex(
                name: "IX_RequirementTraces_TargetRequirementId",
                table: "RequirementTraces",
                column: "TargetRequirementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requirements_DocumentSections_SectionId",
                table: "Requirements",
                column: "SectionId",
                principalTable: "DocumentSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requirements_Documents_DocumentId",
                table: "Requirements",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestCaseExecutions_Users_ExecutedBy",
                table: "TestCaseExecutions",
                column: "ExecutedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestCases_TestSuites_SuiteId",
                table: "TestCases",
                column: "SuiteId",
                principalTable: "TestSuites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestRuns_TestPlans_TestPlanId",
                table: "TestRuns",
                column: "TestPlanId",
                principalTable: "TestPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requirements_DocumentSections_SectionId",
                table: "Requirements");

            migrationBuilder.DropForeignKey(
                name: "FK_Requirements_Documents_DocumentId",
                table: "Requirements");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCaseExecutions_Users_ExecutedBy",
                table: "TestCaseExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_TestCases_TestSuites_SuiteId",
                table: "TestCases");

            migrationBuilder.DropForeignKey(
                name: "FK_TestRuns_TestPlans_TestPlanId",
                table: "TestRuns");

            migrationBuilder.DropTable(
                name: "DocumentSections");

            migrationBuilder.DropTable(
                name: "RequirementTraces");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Requirements_DocumentId",
                table: "Requirements");

            migrationBuilder.DropIndex(
                name: "IX_Requirements_SectionId",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Requirements");

            migrationBuilder.AddForeignKey(
                name: "FK_TestCaseExecutions_Users_ExecutedBy",
                table: "TestCaseExecutions",
                column: "ExecutedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TestCases_TestSuites_SuiteId",
                table: "TestCases",
                column: "SuiteId",
                principalTable: "TestSuites",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TestRuns_TestPlans_TestPlanId",
                table: "TestRuns",
                column: "TestPlanId",
                principalTable: "TestPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
