using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tickets.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookingForEventCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NumberOfDependants",
                table: "Bookings",
                newName: "NumberOfVisitors");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "AttendeeEmail",
                table: "Bookings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AttendeeImageUrl",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendeeName",
                table: "Bookings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AttendeePhone",
                table: "Bookings",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventId",
                table: "Bookings",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_EventId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AttendeeEmail",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AttendeeImageUrl",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AttendeeName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AttendeePhone",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "NumberOfVisitors",
                table: "Bookings",
                newName: "NumberOfDependants");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
