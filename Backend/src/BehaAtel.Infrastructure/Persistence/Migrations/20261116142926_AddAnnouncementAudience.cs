using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BehaAtel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementAudience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Audience",
                table: "Announcements",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "AllResidents");

            migrationBuilder.AddColumn<Guid>(
                name: "BuildingId",
                table: "Announcements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid[]>(
                name: "ResidentIds",
                table: "Announcements",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Audience",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "ResidentIds",
                table: "Announcements");
        }
    }
}
