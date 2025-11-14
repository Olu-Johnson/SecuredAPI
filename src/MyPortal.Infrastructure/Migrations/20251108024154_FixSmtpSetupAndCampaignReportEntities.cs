using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSmtpSetupAndCampaignReportEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmtpSetups_Statuses_StatusId",
                table: "SmtpSetups");

            migrationBuilder.DropIndex(
                name: "IX_SmtpSetups_StatusId",
                table: "SmtpSetups");

            migrationBuilder.DropColumn(
                name: "FromEmail",
                table: "SmtpSetups");

            migrationBuilder.DropColumn(
                name: "FromName",
                table: "SmtpSetups");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "SmtpSetups");

            migrationBuilder.RenameColumn(
                name: "EnableSsl",
                table: "SmtpSetups",
                newName: "isSecure");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "SecuritySetups",
                newName: "RejectedPage");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SecuritySetups",
                newName: "PlatformSetup");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "CampaignReports",
                newName: "lastname");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "CampaignReports",
                newName: "firstname");

            migrationBuilder.RenameColumn(
                name: "BrowserName",
                table: "CampaignReports",
                newName: "broswerName");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedPage",
                table: "SecuritySetups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMonitor",
                table: "SecuritySetups",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "SecuritySetups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Guarantors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guarantors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guarantors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LeaveDays = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaves_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Guarantors_UserId",
                table: "Guarantors",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_UserId",
                table: "Leaves",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guarantors");

            migrationBuilder.DropTable(
                name: "Leaves");

            migrationBuilder.DropColumn(
                name: "ApprovedPage",
                table: "SecuritySetups");

            migrationBuilder.DropColumn(
                name: "IsMonitor",
                table: "SecuritySetups");

            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "SecuritySetups");

            migrationBuilder.RenameColumn(
                name: "isSecure",
                table: "SmtpSetups",
                newName: "EnableSsl");

            migrationBuilder.RenameColumn(
                name: "RejectedPage",
                table: "SecuritySetups",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "PlatformSetup",
                table: "SecuritySetups",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "lastname",
                table: "CampaignReports",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "firstname",
                table: "CampaignReports",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "broswerName",
                table: "CampaignReports",
                newName: "BrowserName");

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

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "SmtpSetups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SmtpSetups_StatusId",
                table: "SmtpSetups",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_SmtpSetups_Statuses_StatusId",
                table: "SmtpSetups",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
