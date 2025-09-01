using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailInventory.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRestoreAuditToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RestoredAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestoredBy",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestoredAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RestoredBy",
                table: "Products");
        }
    }
}
