using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuanyuFlowerShop.Migrations
{
    /// <inheritdoc />
    public partial class AddChatAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentContentType",
                table: "SupportMessages",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentName",
                table: "SupportMessages",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AttachmentSize",
                table: "SupportMessages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentStorageName",
                table: "SupportMessages",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentContentType",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "AttachmentName",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "AttachmentSize",
                table: "SupportMessages");

            migrationBuilder.DropColumn(
                name: "AttachmentStorageName",
                table: "SupportMessages");
        }
    }
}
