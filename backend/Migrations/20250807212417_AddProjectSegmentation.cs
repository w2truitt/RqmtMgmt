using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSegmentation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "TestSuites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "TestPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProjectCode",
                table: "Requirements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Requirements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTeamMembers",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTeamMembers", x => new { x.ProjectId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ProjectTeamMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTeamMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OwnerId",
                table: "Projects",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeamMembers_UserId",
                table: "ProjectTeamMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requirements_Projects_ProjectId",
                table: "Requirements",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestPlans_Projects_ProjectId",
                table: "TestPlans",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestSuites_Projects_ProjectId",
                table: "TestSuites",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requirements_Projects_ProjectId",
                table: "Requirements");

            migrationBuilder.DropForeignKey(
                name: "FK_TestPlans_Projects_ProjectId",
                table: "TestPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TestSuites_Projects_ProjectId",
                table: "TestSuites");

            migrationBuilder.DropTable(
                name: "ProjectTeamMembers");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_TestSuites_ProjectId",
                table: "TestSuites");

            migrationBuilder.DropIndex(
                name: "IX_TestPlans_ProjectId",
                table: "TestPlans");

            migrationBuilder.DropIndex(
                name: "IX_Requirements_ProjectId",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "TestSuites");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "TestPlans");

            migrationBuilder.DropColumn(
                name: "ProjectCode",
                table: "Requirements");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Requirements");
        }
    }
}
