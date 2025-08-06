using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTestExecutionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestRunSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestPlanId = table.Column<int>(type: "int", nullable: false),
                    ExecutedBy = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildVersion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRunSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestRunSessions_TestPlans_TestPlanId",
                        column: x => x.TestPlanId,
                        principalTable: "TestPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestRunSessions_Users_ExecutedBy",
                        column: x => x.ExecutedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestCaseExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestRunSessionId = table.Column<int>(type: "int", nullable: false),
                    TestCaseId = table.Column<int>(type: "int", nullable: false),
                    OverallResult = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutedBy = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefectId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCaseExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCaseExecutions_TestCases_TestCaseId",
                        column: x => x.TestCaseId,
                        principalTable: "TestCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestCaseExecutions_TestRunSessions_TestRunSessionId",
                        column: x => x.TestRunSessionId,
                        principalTable: "TestRunSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestCaseExecutions_Users_ExecutedBy",
                        column: x => x.ExecutedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TestStepExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestCaseExecutionId = table.Column<int>(type: "int", nullable: false),
                    TestStepId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActualResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestStepExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestStepExecutions_TestCaseExecutions_TestCaseExecutionId",
                        column: x => x.TestCaseExecutionId,
                        principalTable: "TestCaseExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestStepExecutions_TestSteps_TestStepId",
                        column: x => x.TestStepId,
                        principalTable: "TestSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseExecutions_ExecutedAt",
                table: "TestCaseExecutions",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseExecutions_ExecutedBy",
                table: "TestCaseExecutions",
                column: "ExecutedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseExecutions_OverallResult",
                table: "TestCaseExecutions",
                column: "OverallResult");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseExecutions_TestCaseId",
                table: "TestCaseExecutions",
                column: "TestCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseExecutions_TestRunSessionId",
                table: "TestCaseExecutions",
                column: "TestRunSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRunSessions_ExecutedBy",
                table: "TestRunSessions",
                column: "ExecutedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TestRunSessions_StartedAt",
                table: "TestRunSessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TestRunSessions_Status",
                table: "TestRunSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TestRunSessions_TestPlanId",
                table: "TestRunSessions",
                column: "TestPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TestStepExecutions_TestCaseExecutionId",
                table: "TestStepExecutions",
                column: "TestCaseExecutionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestStepExecutions_TestStepId",
                table: "TestStepExecutions",
                column: "TestStepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestStepExecutions");

            migrationBuilder.DropTable(
                name: "TestCaseExecutions");

            migrationBuilder.DropTable(
                name: "TestRunSessions");
        }
    }
}
