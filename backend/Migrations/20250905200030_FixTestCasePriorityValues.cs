using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixTestCasePriorityValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure all TestCase records have valid Priority values
            // Set any NULL or invalid Priority values to Medium (2)
            migrationBuilder.Sql("UPDATE TestCases SET Priority = 2 WHERE Priority IS NULL OR Priority < 0 OR Priority > 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
