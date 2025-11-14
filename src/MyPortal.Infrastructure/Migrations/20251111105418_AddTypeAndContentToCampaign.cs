using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeAndContentToCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "CampaignDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "CampaignDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "CampaignDetails");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "CampaignDetails");
        }
    }
}
