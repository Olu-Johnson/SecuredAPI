using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeAndModuleToEmailTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Module",
                table: "EmailTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "EmailTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Module",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "EmailTemplates");
        }
    }
}
