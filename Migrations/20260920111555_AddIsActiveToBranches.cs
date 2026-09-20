using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBAppBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Branches",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Branches");
        }
    }
}
