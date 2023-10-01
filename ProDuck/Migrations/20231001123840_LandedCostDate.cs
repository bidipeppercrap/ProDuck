﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProDuck.Migrations
{
    /// <inheritdoc />
    public partial class LandedCostDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "LandedCosts",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "LandedCosts");
        }
    }
}
