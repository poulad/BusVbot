using System;
using BusV.Telegram.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BusV.Telegram.Data.Migrations
{
    [DbContext(typeof(BusVbotDbContext))]
    partial class MyTtcDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("MyTTCBot.Models.Agency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnName("country")
                        .HasMaxLength(15);

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasAnnotation("Npgsql:DefaultValueSql", "NOW()");

                    b.Property<double>("MaxLatitude")
                        .HasColumnName("lat_max");

                    b.Property<double>("MaxLongitude")
                        .HasColumnName("lon_max");

                    b.Property<double>("MinLatitude")
                        .HasColumnName("lat_min");

                    b.Property<double>("MinLongitude")
                        .HasColumnName("lon_min");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnName("modified_at");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnName("region")
                        .HasMaxLength(25);

                    b.Property<string>("ShortTitle")
                        .HasColumnName("short_title")
                        .HasMaxLength(25);

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnName("tag")
                        .HasMaxLength(25);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasMaxLength(70);

                    b.HasKey("Id");

                    b.ToTable("agency");
                });

            modelBuilder.Entity("MyTTCBot.Models.AgencyRoute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("AgencyId")
                        .HasColumnName("agency_id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<double>("MaxLatitude")
                        .HasColumnName("lat_max");

                    b.Property<double>("MaxLongitude")
                        .HasColumnName("lon_max");

                    b.Property<double>("MinLatitude")
                        .HasColumnName("lat_min");

                    b.Property<double>("MinLongitude")
                        .HasColumnName("lon_min");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnName("modified_at");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnName("tag")
                        .HasMaxLength(25);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.HasIndex("AgencyId", "Tag")
                        .IsUnique();

                    b.ToTable("agency_route");
                });

            modelBuilder.Entity("MyTTCBot.Models.BusStop", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<double>("Latitude")
                        .HasColumnName("lat");

                    b.Property<double>("Longitude")
                        .HasColumnName("lon");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnName("modified_at");

                    b.Property<int?>("StopId")
                        .HasColumnName("stop_id");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnName("tag")
                        .HasMaxLength(35);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.ToTable("bus_stop");
                });

            modelBuilder.Entity("MyTTCBot.Models.FrequentLocation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasAnnotation("Npgsql:DefaultValueSql", "NOW()");

                    b.Property<double>("Latitude")
                        .HasColumnName("lat");

                    b.Property<double>("Longitude")
                        .HasColumnName("lon");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasMaxLength(20);

                    b.Property<int>("UserChatContextId")
                        .HasColumnName("userchat_context_id");

                    b.HasKey("Id");

                    b.HasIndex("UserChatContextId");

                    b.ToTable("frequent_location");
                });

            modelBuilder.Entity("MyTTCBot.Models.RouteDirection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnName("modified_at");

                    b.Property<string>("Name")
                        .HasColumnName("name")
                        .HasMaxLength(200);

                    b.Property<int>("RouteId")
                        .HasColumnName("route_id");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnName("tag")
                        .HasMaxLength(25);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.HasIndex("RouteId", "Tag")
                        .IsUnique();

                    b.ToTable("route_direction");
                });

            modelBuilder.Entity("MyTTCBot.Models.RouteDirectionBusStop", b =>
                {
                    b.Property<int>("BusDirectionId")
                        .HasColumnName("dir_id");

                    b.Property<int>("BusStopId")
                        .HasColumnName("stop_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasAnnotation("Npgsql:DefaultValueSql", "NOW()");

                    b.HasKey("BusDirectionId", "BusStopId");

                    b.HasIndex("BusStopId");

                    b.ToTable("route_direction__bus_stop");
                });

            modelBuilder.Entity("MyTTCBot.Models.UserChatContext", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<int>("AgencyId")
                        .HasColumnName("agency_id");

                    b.Property<long>("ChatId")
                        .HasColumnName("chat_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasAnnotation("Npgsql:DefaultValueSql", "NOW()");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnName("modified_at");

                    b.Property<long>("UserId")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("AgencyId");

                    b.HasIndex("UserId", "ChatId")
                        .IsUnique();

                    b.ToTable("userchat_context");
                });

            modelBuilder.Entity("MyTTCBot.Models.AgencyRoute", b =>
                {
                    b.HasOne("MyTTCBot.Models.Agency", "Agency")
                        .WithMany("Routes")
                        .HasForeignKey("AgencyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTTCBot.Models.FrequentLocation", b =>
                {
                    b.HasOne("MyTTCBot.Models.UserChatContext", "UserChatContext")
                        .WithMany("FrequentLocations")
                        .HasForeignKey("UserChatContextId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTTCBot.Models.RouteDirection", b =>
                {
                    b.HasOne("MyTTCBot.Models.AgencyRoute", "Route")
                        .WithMany("Directions")
                        .HasForeignKey("RouteId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTTCBot.Models.RouteDirectionBusStop", b =>
                {
                    b.HasOne("MyTTCBot.Models.RouteDirection", "Direction")
                        .WithMany("RouteDirectionBusStops")
                        .HasForeignKey("BusDirectionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MyTTCBot.Models.BusStop", "Stop")
                        .WithMany("RouteDirectionBusStops")
                        .HasForeignKey("BusStopId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTTCBot.Models.UserChatContext", b =>
                {
                    b.HasOne("MyTTCBot.Models.Agency", "Agency")
                        .WithMany()
                        .HasForeignKey("AgencyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
