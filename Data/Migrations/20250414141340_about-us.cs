using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LehmanCustomConstruction.Migrations
{
    /// <inheritdoc />
    public partial class aboutus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageContents",
                columns: table => new
                {
                    PageKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageContents", x => x.PageKey);
                });

            migrationBuilder.InsertData(
                table: "PageContents",
                columns: new[] { "PageKey", "DateModified", "HtmlContent" },
                values: new object[] { "AboutUsMain", new DateTime(2025, 4, 14, 14, 13, 38, 906, DateTimeKind.Utc).AddTicks(6632), "<p>Lehman Custom Construction is built on a foundation of quality, integrity, and partnership. Founded by Tom Lehman, our passion lies in translating your vision into a home that is both uniquely yours and built to the highest standards of craftsmanship.</p><p>We believe the custom home building process should be collaborative and transparent. From initial concept sketches to the final walkthrough, we work closely with you, ensuring every detail reflects your lifestyle and preferences.</p><h2>Our Approach</h2><p>Our approach combines time-honored building techniques with modern innovations. We partner with skilled architects, designers, and tradespeople who share our commitment to excellence. Key elements include:</p><ul><li><strong>Personalized Design:</strong> Tailoring every aspect to your needs.</li><li><strong>Quality Materials:</strong> Sourcing durable and beautiful materials.</li><li><strong>Transparent Communication:</strong> Keeping you informed every step of the way.</li><li><strong>Expert Project Management:</strong> Ensuring timelines and budgets are respected.</li></ul><p>Building a custom home is a significant journey, and we are honored to be considered as your guide and partner.</p>" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageContents");
        }
    }
}
