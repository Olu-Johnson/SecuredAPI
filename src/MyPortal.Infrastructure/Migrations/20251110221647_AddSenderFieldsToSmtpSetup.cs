using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSenderFieldsToSmtpSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Encryption",
                table: "SmtpSetups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromEmail",
                table: "SmtpSetups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromName",
                table: "SmtpSetups",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Encryption",
                table: "SmtpSetups");

            migrationBuilder.DropColumn(
                name: "FromEmail",
                table: "SmtpSetups");

            migrationBuilder.DropColumn(
                name: "FromName",
                table: "SmtpSetups");
        }
    }
}
