using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHierarchicalSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DocumentId",
                table: "DocumentSections",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "DocumentSections",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "ParentSectionId",
                table: "DocumentSections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionNumber",
                table: "DocumentSections",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Update existing sections to have Level = 1
            migrationBuilder.Sql(
                "UPDATE DocumentSections SET Level = 1 WHERE ParentSectionId IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSections_Level",
                table: "DocumentSections",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSections_ParentSectionId_SectionOrder",
                table: "DocumentSections",
                columns: new[] { "ParentSectionId", "SectionOrder" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_DocumentSection_ParentReference",
                table: "DocumentSections",
                sql: "([DocumentId] IS NOT NULL) OR ([ParentSectionId] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentSections_DocumentSections_ParentSectionId",
                table: "DocumentSections",
                column: "ParentSectionId",
                principalTable: "DocumentSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentSections_DocumentSections_ParentSectionId",
                table: "DocumentSections");

            migrationBuilder.DropIndex(
                name: "IX_DocumentSections_Level",
                table: "DocumentSections");

            migrationBuilder.DropIndex(
                name: "IX_DocumentSections_ParentSectionId_SectionOrder",
                table: "DocumentSections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DocumentSection_ParentReference",
                table: "DocumentSections");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "DocumentSections");

            migrationBuilder.DropColumn(
                name: "ParentSectionId",
                table: "DocumentSections");

            migrationBuilder.DropColumn(
                name: "SectionNumber",
                table: "DocumentSections");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentId",
                table: "DocumentSections",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
