using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBAppBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "SystemUsers");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "SystemUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "SystemUsers");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "SystemUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
