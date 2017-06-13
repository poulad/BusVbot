using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyTTCBot.Migrations
{
    public partial class AddUserChatContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "userchat_context",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    chat_id = table.Column<long>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "NOW()"),
                    user_id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userchat_context", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "frequent_location",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "NOW()"),
                    lat = table.Column<double>(nullable: false),
                    lon = table.Column<double>(nullable: false),
                    name = table.Column<string>(maxLength: 20, nullable: false),
                    userchat_context_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frequent_location", x => x.id);
                    table.ForeignKey(
                        name: "FK_frequent_location_userchat_context_userchat_context_id",
                        column: x => x.userchat_context_id,
                        principalTable: "userchat_context",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_frequent_location_userchat_context_id",
                table: "frequent_location",
                column: "userchat_context_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "frequent_location");

            migrationBuilder.DropTable(
                name: "userchat_context");
        }
    }
}
