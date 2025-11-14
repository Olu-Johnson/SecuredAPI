using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchConfigAndRetryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchIntervalMinutes",
                table: "SmtpSetups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BatchSize",
                table: "SmtpSetups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSendingEnabled",
                table: "SmtpSetups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetryAttempts",
                table: "SmtpSetups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAttemptDate",
                table: "Emails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentDate",
                table: "Emails",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchIntervalMinutes",
                table: "SmtpSetups");

            migrationBuilder.DropColumn(
                name: "BatchSize",
                table: "SmtpSetups");

            migrationBuilder.DropColumn(
                name: "IsSendingEnabled",
                table: "SmtpSetups");

            migrationBuilder.DropColumn(
                name: "MaxRetryAttempts",
                table: "SmtpSetups");

            migrationBuilder.DropColumn(
                name: "LastAttemptDate",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "SentDate",
                table: "Emails");
        }
    }
}
