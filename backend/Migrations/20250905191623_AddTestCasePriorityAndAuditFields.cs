using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTestCasePriorityAndAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "TestCases",
                type: "int",
                nullable: false,
                defaultValue: 2); // Default to Medium (TestCasePriority.Medium = 2)

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TestCases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "TestCases",
                type: "int",
                nullable: true);

            // Update existing records to have Medium priority
            migrationBuilder.Sql("UPDATE TestCases SET Priority = 2 WHERE Priority = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_UpdatedBy",
                table: "TestCases",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_TestCases_Users_UpdatedBy",
                table: "TestCases",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestCases_Users_UpdatedBy",
                table: "TestCases");

            migrationBuilder.DropIndex(
                name: "IX_TestCases_UpdatedBy",
                table: "TestCases");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "TestCases");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TestCases");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TestCases");
        }
    }
}
