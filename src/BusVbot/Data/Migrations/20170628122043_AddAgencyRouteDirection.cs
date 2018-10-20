using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace BusVbot.Data.Migrations
{
    public partial class AddAgencyRouteDirection : Migration
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
                    title = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_route", x => x.id);
                    table.ForeignKey(
                        name: "FK_agency_route_agency_agency_id",
                        column: x => x.agency_id,
                        principalTable: "agency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    modified_at = table.Column<DateTime>(nullable: true),
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
                name: "route_direction",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
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
                name: "IX_agency_route_agency_id_tag",
                table: "agency_route",
                columns: new[] { "agency_id", "tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_frequent_location_userchat_context_id",
                table: "frequent_location",
                column: "userchat_context_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_direction_route_id_tag",
                table: "route_direction",
                columns: new[] { "route_id", "tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_route_direction__bus_stop_stop_id",
                table: "route_direction__bus_stop",
                column: "stop_id");

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
                name: "route_direction__bus_stop");

            migrationBuilder.DropTable(
                name: "userchat_context");

            migrationBuilder.DropTable(
                name: "route_direction");

            migrationBuilder.DropTable(
                name: "bus_stop");

            migrationBuilder.DropTable(
                name: "agency_route");

            migrationBuilder.DropTable(
                name: "agency");
        }
    }
}
