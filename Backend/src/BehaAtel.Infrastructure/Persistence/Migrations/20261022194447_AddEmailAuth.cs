using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BehaAtel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RegistrationBirthDate",
                table: "Users",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationOrganization",
                table: "Users",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationPassportNumber",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationRequestedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Users"
                SET "EmailConfirmed" = TRUE,
                    "ApprovalStatus" = 'Approved';

                INSERT INTO "Settings" ("Key", "Value", "UpdatedAt")
                VALUES
                    ('Auth.TwoFactorEmail.Admins', 'false', CURRENT_TIMESTAMP),
                    ('Auth.TwoFactorEmail.Residents', 'false', CURRENT_TIMESTAMP)
                ON CONFLICT ("Key") DO NOTHING;
                """);

            migrationBuilder.CreateTable(
                name: "EmailCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailCodes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailCodes_ExpiresAt",
                table: "EmailCodes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailCodes_UserId_Purpose",
                table: "EmailCodes",
                columns: new[] { "UserId", "Purpose" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailCodes");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistrationBirthDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistrationOrganization",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistrationPassportNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistrationRequestedAt",
                table: "Users");
        }
    }
}
