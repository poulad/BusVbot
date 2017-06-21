using Microsoft.EntityFrameworkCore;

namespace MyTTCBot.Models
{
    public class MyTtcDbContext : DbContext
    {
        public DbSet<Agency> Agencies { get; set; }

        public DbSet<UserChatContext> UserChatContexts { get; set; }

        public DbSet<FrequentLocation> FrequentLocations { get; set; }

        public MyTtcDbContext(DbContextOptions<MyTtcDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Table "userchat_context"

            modelBuilder.Entity<UserChatContext>()
                .HasIndex(x => new { x.UserId, x.ChatId })
                .IsUnique();

            modelBuilder.Entity<UserChatContext>()
                .Property(x => x.CreatedAt)
                .ForNpgsqlHasDefaultValueSql("NOW()");

            #endregion

            modelBuilder.Entity<FrequentLocation>()
                .Property(x => x.CreatedAt)
                .ForNpgsqlHasDefaultValueSql("NOW()");

            modelBuilder.Entity<Agency>()
                .Property(x => x.CreatedAt)
                .ForNpgsqlHasDefaultValueSql("NOW()");
        }
    }
}
