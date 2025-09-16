using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequirementTypeEnumValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing enum values from old format (CRS/PRS) to new format (CRD/PRD)
            migrationBuilder.Sql("UPDATE Requirements SET Type = 'CRD' WHERE Type = 'CRS'");
            migrationBuilder.Sql("UPDATE Requirements SET Type = 'PRD' WHERE Type = 'PRS'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert enum values back to old format
            migrationBuilder.Sql("UPDATE Requirements SET Type = 'CRS' WHERE Type = 'CRD'");
            migrationBuilder.Sql("UPDATE Requirements SET Type = 'PRS' WHERE Type = 'PRD'");
        }
    }
}