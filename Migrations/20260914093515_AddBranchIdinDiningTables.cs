using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBAppBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdinDiningTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "DiningTables",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "DiningTables");
        }
    }
}
