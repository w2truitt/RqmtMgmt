using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectBasedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestSuites_ProjectId",
                table: "TestSuites");

            migrationBuilder.DropIndex(
                name: "IX_TestPlans_ProjectId",
                table: "TestPlans");

            migrationBuilder.DropIndex(
                name: "IX_Requirements_ProjectId",
                table: "Requirements");

            migrationBuilder.CreateIndex(
                name: "IX_TestSuites_ProjectId_Id",
                table: "TestSuites",
                columns: new[] { "ProjectId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TestPlans_ProjectId_Id",
                table: "TestPlans",
                columns: new[] { "ProjectId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_ProjectId_Id",
                table: "Requirements",
                columns: new[] { "ProjectId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestSuites_ProjectId_Id",
                table: "TestSuites");

            migrationBuilder.DropIndex(
                name: "IX_TestPlans_ProjectId_Id",
                table: "TestPlans");

            migrationBuilder.DropIndex(
                name: "IX_Requirements_ProjectId_Id",
                table: "Requirements");

            migrationBuilder.CreateIndex(
                name: "IX_TestSuites_ProjectId",
                table: "TestSuites",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TestPlans_ProjectId",
                table: "TestPlans",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_ProjectId",
                table: "Requirements",
                column: "ProjectId");
        }
    }
}
