using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProDuck.Migrations
{
    /// <inheritdoc />
    public partial class LandedCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PurchaseOrders",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LandedCosts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Biller = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDelivered = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandedCosts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LandedCostItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PurchaseOrderId = table.Column<long>(type: "bigint", nullable: false),
                    LandedCostId = table.Column<long>(type: "bigint", nullable: true),
                    LandedCostItemId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandedCostItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandedCostItems_LandedCostItems_LandedCostItemId",
                        column: x => x.LandedCostItemId,
                        principalTable: "LandedCostItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LandedCostItems_LandedCosts_LandedCostId",
                        column: x => x.LandedCostId,
                        principalTable: "LandedCosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LandedCostItems_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LandedCostItems_LandedCostId",
                table: "LandedCostItems",
                column: "LandedCostId");

            migrationBuilder.CreateIndex(
                name: "IX_LandedCostItems_LandedCostItemId",
                table: "LandedCostItems",
                column: "LandedCostItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LandedCostItems_PurchaseOrderId",
                table: "LandedCostItems",
                column: "PurchaseOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandedCostItems");

            migrationBuilder.DropTable(
                name: "LandedCosts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PurchaseOrders");
        }
    }
}
