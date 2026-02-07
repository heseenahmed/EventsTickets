using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickets.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCheckoutLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF COL_LENGTH('Tickets', 'TotalPrice') IS NULL BEGIN ALTER TABLE Tickets ADD TotalPrice decimal(18,2) NOT NULL DEFAULT 0.0 END");
            migrationBuilder.Sql("IF COL_LENGTH('Events', 'VisitorFee') IS NULL BEGIN ALTER TABLE Events ADD VisitorFee decimal(18,2) NOT NULL DEFAULT 0.0 END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "VisitorFee",
                table: "Events");
        }
    }
}
