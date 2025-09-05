using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTestCaseSuiteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestCases_SuiteId",
                table: "TestCases");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_SuiteId_Id",
                table: "TestCases",
                columns: new[] { "SuiteId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestCases_SuiteId_Id",
                table: "TestCases");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_SuiteId",
                table: "TestCases",
                column: "SuiteId");
        }
    }
}
