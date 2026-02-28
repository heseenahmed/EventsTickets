using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickets.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyDepartmentYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Tickets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Faculty",
                table: "Tickets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "Tickets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Faculty",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Tickets");
        }
    }
}
