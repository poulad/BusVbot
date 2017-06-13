using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MyTTCBot.Models;

namespace MyTTCBot.Migrations
{
    [DbContext(typeof(MyTtcDbContext))]
    [Migration("20170609180243_AddUserChatContext")]
    partial class AddUserChatContext
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("MyTTCBot.Models.FrequentLocation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("NOW()");

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

                    b.Property<long>("ChatId")
                        .HasColumnName("chat_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("NOW()");

                    b.Property<long>("UserId")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.ToTable("userchat_context");
                });

            modelBuilder.Entity("MyTTCBot.Models.FrequentLocation", b =>
                {
                    b.HasOne("MyTTCBot.Models.UserChatContext", "UserChatContext")
                        .WithMany("FrequentLocations")
                        .HasForeignKey("UserChatContextId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
