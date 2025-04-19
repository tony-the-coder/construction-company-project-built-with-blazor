using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LehmanCustomConstruction.Migrations
{
    /// <inheritdoc />
    public partial class FixPageContentSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PageContents",
                keyColumn: "PageKey",
                keyValue: "AboutUsMain",
                column: "DateModified",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PageContents",
                keyColumn: "PageKey",
                keyValue: "AboutUsMain",
                column: "DateModified",
                value: new DateTime(2025, 4, 14, 14, 13, 38, 906, DateTimeKind.Utc).AddTicks(6632));
        }
    }
}
