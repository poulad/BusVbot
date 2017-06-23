using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyTTCBot.Data.Migrations
{
    public partial class AddRoutesDirectionsStops : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userchat_context_agency_agency_id",
                table: "userchat_context");

            migrationBuilder.DropTable(
                name: "agency");

            migrationBuilder.CreateTable(
                name: "bus_stop",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_at = table.Column<DateTime>(nullable: false),
                    lat = table.Column<double>(nullable: false),
                    lon = table.Column<double>(nullable: false),
                    modified_at = table.Column<DateTime>(nullable: true),
                    stop_id = table.Column<int>(nullable: true),
                    tag = table.Column<string>(maxLength: 35, nullable: false),
                    title = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bus_stop", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transit_agency",
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
                    table.PrimaryKey("PK_transit_agency", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agency_route",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    agency_id = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    lat_max = table.Column<double>(nullable: false),
                    lon_max = table.Column<double>(nullable: false),
                    lat_min = table.Column<double>(nullable: false),
                    lon_min = table.Column<double>(nullable: false),
                    modified_at = table.Column<DateTime>(nullable: true),
                    tag = table.Column<string>(maxLength: 25, nullable: false),
                    title = table.Column<string>(maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_route", x => x.id);
                    table.ForeignKey(
                        name: "FK_agency_route_transit_agency_agency_id",
                        column: x => x.agency_id,
                        principalTable: "transit_agency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "route_direction",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    branch = table.Column<string>(maxLength: 15, nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    modified_at = table.Column<DateTime>(nullable: true),
                    name = table.Column<string>(maxLength: 200, nullable: true),
                    route_id = table.Column<int>(nullable: false),
                    tag = table.Column<string>(maxLength: 25, nullable: false),
                    title = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_direction", x => x.id);
                    table.ForeignKey(
                        name: "FK_route_direction_agency_route_route_id",
                        column: x => x.route_id,
                        principalTable: "agency_route",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "route_direction__bus_stop",
                columns: table => new
                {
                    dir_id = table.Column<int>(nullable: false),
                    stop_id = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_direction__bus_stop", x => new { x.dir_id, x.stop_id });
                    table.ForeignKey(
                        name: "FK_route_direction__bus_stop_route_direction_dir_id",
                        column: x => x.dir_id,
                        principalTable: "route_direction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_route_direction__bus_stop_bus_stop_stop_id",
                        column: x => x.stop_id,
                        principalTable: "bus_stop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agency_route_agency_id",
                table: "agency_route",
                column: "agency_id");

            migrationBuilder.CreateIndex(
                name: "IX_agency_route_tag",
                table: "agency_route",
                column: "tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bus_stop_tag",
                table: "bus_stop",
                column: "tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_route_direction_route_id",
                table: "route_direction",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_direction_tag",
                table: "route_direction",
                column: "tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_route_direction__bus_stop_stop_id",
                table: "route_direction__bus_stop",
                column: "stop_id");

            migrationBuilder.AddForeignKey(
                name: "FK_userchat_context_transit_agency_agency_id",
                table: "userchat_context",
                column: "agency_id",
                principalTable: "transit_agency",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userchat_context_transit_agency_agency_id",
                table: "userchat_context");

            migrationBuilder.DropTable(
                name: "route_direction__bus_stop");

            migrationBuilder.DropTable(
                name: "route_direction");

            migrationBuilder.DropTable(
                name: "bus_stop");

            migrationBuilder.DropTable(
                name: "agency_route");

            migrationBuilder.DropTable(
                name: "transit_agency");

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

            migrationBuilder.AddForeignKey(
                name: "FK_userchat_context_agency_agency_id",
                table: "userchat_context",
                column: "agency_id",
                principalTable: "agency",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
