using Microsoft.EntityFrameworkCore;

namespace MyTTCBot.Models
{
    public class MyTtcDbContext : DbContext
    {
        public DbSet<UserChatContext> UserChatContexts { get; set; }

        public DbSet<FrequentLocation> FrequentLocations { get; set; }

        public MyTtcDbContext(DbContextOptions<MyTtcDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserChatContext>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<FrequentLocation>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");
        }
    }
}
