using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyTTCBot.Data.Migrations
{
    public partial class AddAgency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agency",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    country = table.Column<string>(maxLength: 15, nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "NOW()"),
                    lat_max = table.Column<double>(nullable: false),
                    lon_max = table.Column<double>(nullable: false),
                    lat_min = table.Column<double>(nullable: false),
                    lon_min = table.Column<double>(nullable: false),
                    modified_at = table.Column<DateTime>(nullable: true),
                    region = table.Column<string>(maxLength: 25, nullable: false),
                    short_title = table.Column<string>(maxLength: 25, nullable: true),
                    tag = table.Column<string>(maxLength: 25, nullable: false),
                    title = table.Column<string>(maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "userchat_context",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    agency_id = table.Column<int>(nullable: false),
                    chat_id = table.Column<long>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "NOW()"),
                    user_id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userchat_context", x => x.id);
                    table.ForeignKey(
                        name: "FK_userchat_context_agency_agency_id",
                        column: x => x.agency_id,
                        principalTable: "agency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_userchat_context_agency_id",
                table: "userchat_context",
                column: "agency_id");

            migrationBuilder.CreateIndex(
                name: "IX_userchat_context_user_id_chat_id",
                table: "userchat_context",
                columns: new[] { "user_id", "chat_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "frequent_location");

            migrationBuilder.DropTable(
                name: "userchat_context");

            migrationBuilder.DropTable(
                name: "agency");
        }
    }
}
