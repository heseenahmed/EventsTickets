using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickets.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class EventsConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventDetails",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsOfEntries",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventDetails",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TermsOfEntries",
                table: "Events");
        }
    }
}
