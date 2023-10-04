using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProDuck.Migrations
{
    /// <inheritdoc />
    public partial class ParentLandedCostItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LandedCostItems_PurchaseOrders_PurchaseOrderId",
                table: "LandedCostItems");

            migrationBuilder.AlterColumn<long>(
                name: "PurchaseOrderId",
                table: "LandedCostItems",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_LandedCostItems_PurchaseOrders_PurchaseOrderId",
                table: "LandedCostItems",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LandedCostItems_PurchaseOrders_PurchaseOrderId",
                table: "LandedCostItems");

            migrationBuilder.AlterColumn<long>(
                name: "PurchaseOrderId",
                table: "LandedCostItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LandedCostItems_PurchaseOrders_PurchaseOrderId",
                table: "LandedCostItems",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
