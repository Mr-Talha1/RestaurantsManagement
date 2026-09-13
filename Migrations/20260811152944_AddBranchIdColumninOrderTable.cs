using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBAppBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdColumninOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Orders",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Orders");
        }
    }
}
