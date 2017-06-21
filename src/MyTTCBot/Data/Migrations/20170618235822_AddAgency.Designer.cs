using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MyTTCBot.Models;

namespace MyTTCBot.Data.Migrations
{
    [DbContext(typeof(MyTtcDbContext))]
    [Migration("20170618235822_AddAgency")]
    partial class AddAgency
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.Property<long>("UserId")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("AgencyId");

                    b.HasIndex("UserId", "ChatId")
                        .IsUnique();

                    b.ToTable("userchat_context");
                });

            modelBuilder.Entity("MyTTCBot.Models.FrequentLocation", b =>
                {
                    b.HasOne("MyTTCBot.Models.UserChatContext", "UserChatContext")
                        .WithMany("FrequentLocations")
                        .HasForeignKey("UserChatContextId")
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
