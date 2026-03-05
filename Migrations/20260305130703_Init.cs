using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BIPL_RAASTP2M.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MerchantId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MerchantId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    CustomerPhone = table.Column<string>(type: "text", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "DiningTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MerchantId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiningTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "logs",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<string>(type: "text", nullable: false),
                    Activity = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Application = table.Column<string>(type: "text", nullable: false),
                    Interface = table.Column<string>(type: "text", nullable: false),
                    eDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IPAddress = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    CnicFrontPath = table.Column<string>(type: "text", nullable: true),
                    CnicBackPath = table.Column<string>(type: "text", nullable: true),
                    OrderLimit = table.Column<int>(type: "integer", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    MobileNumber = table.Column<string>(type: "text", nullable: true),
                    LogoPath = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MerchantId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    OrderNumber = table.Column<long>(type: "bigint", nullable: false),
                    OrderType = table.Column<string>(type: "text", nullable: false),
                    TableId = table.Column<int>(type: "integer", nullable: true),
                    PaymentType = table.Column<string>(type: "text", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    GrossTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalDiscount = table.Column<decimal>(type: "numeric", nullable: true),
                    ItemsCount = table.Column<int>(type: "integer", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderDiscountType = table.Column<string>(type: "text", nullable: true),
                    OrderDiscountValue = table.Column<decimal>(type: "numeric", nullable: true),
                    OrderDiscountAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    IsRefunded = table.Column<bool>(type: "boolean", nullable: false),
                    RefundedBy = table.Column<string>(type: "text", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaxType = table.Column<string>(type: "text", nullable: true),
                    TaxValue = table.Column<decimal>(type: "numeric", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    IsEdited = table.Column<bool>(type: "boolean", nullable: false),
                    EditedBy = table.Column<string>(type: "text", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EditCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MerchantId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    ProductPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    ImagePublicId = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MerchantId = table.Column<int>(type: "integer", nullable: false),
                    UserID = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    MobileNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Qty = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountType = table.Column<string>(type: "text", nullable: true),
                    DiscountValue = table.Column<decimal>(type: "numeric", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    GrossTotal = table.Column<decimal>(type: "numeric", nullable: true),
                    AmountAfterDiscount = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrdersId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrdersId",
                        column: x => x.OrdersId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrdersId",
                table: "OrderItems",
                column: "OrdersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "DiningTables");

            migrationBuilder.DropTable(
                name: "logs");

            migrationBuilder.DropTable(
                name: "Merchants");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "SystemUsers");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
