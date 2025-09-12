using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTestRunTestCaseIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestRuns_TestCaseId",
                table: "TestRuns");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_TestCaseId_Id",
                table: "TestRuns",
                columns: new[] { "TestCaseId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestRuns_TestCaseId_Id",
                table: "TestRuns");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_TestCaseId",
                table: "TestRuns",
                column: "TestCaseId");
        }
    }
}
