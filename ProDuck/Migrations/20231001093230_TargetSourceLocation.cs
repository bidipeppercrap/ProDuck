using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProDuck.Migrations
{
    /// <inheritdoc />
    public partial class TargetSourceLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPurchase",
                table: "LandedCosts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "SourceLocationId",
                table: "LandedCosts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TargetLocationId",
                table: "LandedCosts",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LandedCosts_SourceLocationId",
                table: "LandedCosts",
                column: "SourceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LandedCosts_TargetLocationId",
                table: "LandedCosts",
                column: "TargetLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LandedCosts_Locations_SourceLocationId",
                table: "LandedCosts",
                column: "SourceLocationId",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LandedCosts_Locations_TargetLocationId",
                table: "LandedCosts",
                column: "TargetLocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LandedCosts_Locations_SourceLocationId",
                table: "LandedCosts");

            migrationBuilder.DropForeignKey(
                name: "FK_LandedCosts_Locations_TargetLocationId",
                table: "LandedCosts");

            migrationBuilder.DropIndex(
                name: "IX_LandedCosts_SourceLocationId",
                table: "LandedCosts");

            migrationBuilder.DropIndex(
                name: "IX_LandedCosts_TargetLocationId",
                table: "LandedCosts");

            migrationBuilder.DropColumn(
                name: "IsPurchase",
                table: "LandedCosts");

            migrationBuilder.DropColumn(
                name: "SourceLocationId",
                table: "LandedCosts");

            migrationBuilder.DropColumn(
                name: "TargetLocationId",
                table: "LandedCosts");
        }
    }
}
