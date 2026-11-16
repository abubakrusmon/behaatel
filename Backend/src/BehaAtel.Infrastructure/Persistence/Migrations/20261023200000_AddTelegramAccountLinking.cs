using System;
using BehaAtel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace BehaAtel.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261023200000_AddTelegramAccountLinking")]
    public partial class AddTelegramAccountLinking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramChatId",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TelegramLinkExempt",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TelegramLinkAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartCodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PollTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BoundAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramLinkAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramLinkAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramLinkAttempts_ExpiresAt",
                table: "TelegramLinkAttempts",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramLinkAttempts_PollTokenHash",
                table: "TelegramLinkAttempts",
                column: "PollTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramLinkAttempts_StartCodeHash",
                table: "TelegramLinkAttempts",
                column: "StartCodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramLinkAttempts_UserId",
                table: "TelegramLinkAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramChatId",
                table: "Users",
                column: "TelegramChatId",
                unique: true,
                filter: "\"TelegramChatId\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TelegramLinkAttempts");
            migrationBuilder.DropIndex(name: "IX_Users_TelegramChatId", table: "Users");
            migrationBuilder.DropColumn(name: "TelegramChatId", table: "Users");
            migrationBuilder.DropColumn(name: "TelegramLinkExempt", table: "Users");
        }
    }
}
