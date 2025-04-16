using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LehmanCustomConstruction.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDocumentSchemaSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "CustomerDocuments");

            migrationBuilder.DropColumn(
                name: "UniqueFileName",
                table: "CustomerDocuments");

            migrationBuilder.RenameColumn(
                name: "UploadedTimestamp",
                table: "CustomerDocuments",
                newName: "UploadTimestamp");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFileName",
                table: "CustomerDocuments",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "CustomerDocuments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "CustomerDocuments",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "CustomerDocuments");

            migrationBuilder.RenameColumn(
                name: "UploadTimestamp",
                table: "CustomerDocuments",
                newName: "UploadedTimestamp");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFileName",
                table: "CustomerDocuments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(260)",
                oldMaxLength: 260);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "CustomerDocuments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "CustomerDocuments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UniqueFileName",
                table: "CustomerDocuments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
