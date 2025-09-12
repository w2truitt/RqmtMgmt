using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ConvertTestCasePriorityToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, add a temporary column to hold the string values
            migrationBuilder.AddColumn<string>(
                name: "PriorityTemp",
                table: "TestCases",
                type: "nvarchar(max)",
                nullable: true);

            // Convert integer values to string enum names
            // 0 = Critical, 1 = High, 2 = Medium, 3 = Low
            migrationBuilder.Sql(@"
                UPDATE TestCases 
                SET PriorityTemp = CASE Priority
                    WHEN 0 THEN 'Critical'
                    WHEN 1 THEN 'High'
                    WHEN 2 THEN 'Medium'
                    WHEN 3 THEN 'Low'
                    ELSE 'Medium'
                END");

            // Drop the old integer column
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "TestCases");

            // Rename the temporary column to Priority
            migrationBuilder.RenameColumn(
                name: "PriorityTemp",
                table: "TestCases",
                newName: "Priority");

            // Make the new Priority column non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "TestCases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Medium");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "TestCases",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
