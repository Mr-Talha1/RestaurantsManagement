using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBAppBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "Merchants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "Merchants");
        }
    }
}
