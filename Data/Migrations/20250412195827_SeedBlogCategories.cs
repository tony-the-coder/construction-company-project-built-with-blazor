using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LehmanCustomConstruction.Migrations
{
    /// <inheritdoc />
    public partial class SeedBlogCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BlogCategories",
                columns: new[] { "ID", "Name", "Slug" },
                values: new object[,]
                {
                    { 1, "Company News", "company-news" },
                    { 2, "Project Spotlights", "project-spotlights" },
                    { 3, "Design Ideas", "design-ideas" },
                    { 4, "Construction Tips", "construction-tips" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BlogCategories",
                keyColumn: "ID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BlogCategories",
                keyColumn: "ID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BlogCategories",
                keyColumn: "ID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BlogCategories",
                keyColumn: "ID",
                keyValue: 4);
        }
    }
}
