using Microsoft.EntityFrameworkCore;

namespace BusVbot.Models
{
    public class BusVbotDbContext : DbContext
    {
        public DbSet<Agency> Agencies { get; set; }

        public DbSet<AgencyRoute> AgencyRoutes { get; set; }

        public DbSet<RouteDirection> RouteDirections { get; set; }

        public DbSet<BusStop> BusStops { get; set; }

        public DbSet<UserChatContext> UserChatContexts { get; set; }

        public DbSet<FrequentLocation> FrequentLocations { get; set; }

        public BusVbotDbContext(DbContextOptions<BusVbotDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Table "agency"

            modelBuilder.Entity<Agency>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("NOW()");

            #endregion

            #region Table "userchat_context"

            modelBuilder.Entity<UserChatContext>()
                .HasIndex(x => new { x.UserId, x.ChatId })
                .IsUnique();

            modelBuilder.Entity<UserChatContext>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            #endregion

            #region Table "agency_route"

            modelBuilder.Entity<AgencyRoute>()
                .HasIndex(x => new { x.AgencyId, x.Tag })
                .IsUnique();

            #endregion

            #region Table "route_direction"

            modelBuilder.Entity<RouteDirection>()
                .HasIndex(x => new { x.RouteId, x.Tag })
                .IsUnique();

            #endregion

            #region Table "bus_stop"

            //modelBuilder.Entity<BusStop>()
            //    .HasIndex(x => new { x.Tag })
            //    .IsUnique();

            #endregion

            #region Relationship: route_direction *--* bus_stop

            modelBuilder.Entity<RouteDirectionBusStop>()
                .HasKey(x => new { x.BusDirectionId, x.BusStopId });

            modelBuilder.Entity<RouteDirectionBusStop>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<RouteDirectionBusStop>()
                .HasOne(e => e.Direction)
                .WithMany(d => d.RouteDirectionBusStops)
                .HasForeignKey(x => x.BusDirectionId);

            modelBuilder.Entity<RouteDirectionBusStop>()
                .HasOne(e => e.Stop)
                .WithMany(s => s.RouteDirectionBusStops)
                .HasForeignKey(x => x.BusStopId);

            #endregion

            #region Table "frequent_location"

            modelBuilder.Entity<FrequentLocation>()
                .Property(l => l.CreatedAt)
                .HasDefaultValueSql("NOW()");

            #endregion
        }
    }
}
