using Microsoft.EntityFrameworkCore;

namespace MyTTCBot.Models
{
    public class MyTtcDbContext : DbContext
    {
        public DbSet<TransitAgency> TransitAgencies { get; set; }

        public DbSet<AgencyRoute> AgencyRoutes { get; set; }

        public DbSet<RouteDirection> RouteDirections { get; set; }

        public DbSet<BusStop> BusStops { get; set; }

        public DbSet<UserChatContext> UserChatContexts { get; set; }

        public DbSet<FrequentLocation> FrequentLocations { get; set; }

        public MyTtcDbContext(DbContextOptions<MyTtcDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransitAgency>()
                .Property(a => a.CreatedAt)
                .ForNpgsqlHasDefaultValueSql("NOW()");

            #region Table "userchat_context"

            modelBuilder.Entity<UserChatContext>()
                .HasIndex(x => new { x.UserId, x.ChatId })
                .IsUnique();

            modelBuilder.Entity<UserChatContext>()
                .Property(x => x.CreatedAt)
                .ForNpgsqlHasDefaultValueSql("NOW()");

            #endregion

            #region Table "agency_route"

            modelBuilder.Entity<AgencyRoute>()
                .HasIndex(x => new { x.Tag })
                .IsUnique();

            #endregion

            #region Table "route_direction"

            modelBuilder.Entity<RouteDirection>()
                .HasIndex(x => new { x.Tag })
                .IsUnique();

            #endregion

            #region Table "bus_stop"

            modelBuilder.Entity<BusStop>()
                .HasIndex(x => new { x.Tag })
                .IsUnique();

            #endregion

            #region Relationship: route_direction *--* bus_stop

            modelBuilder.Entity<RouteDirectionBusStop>()
                .HasKey(x => new { x.BusDirectionId, x.BusStopId });

            modelBuilder.Entity<RouteDirectionBusStop>()
                .Property(x => x.CreatedAt)
                .ForNpgsqlHasDefaultValueSql("NOW()");

            modelBuilder.Entity<RouteDirectionBusStop>()
                .HasOne(e => e.Direction)
                .WithMany(d => d.RouteDirectionBusStops)
                .HasForeignKey(x => x.BusDirectionId);

            modelBuilder.Entity<RouteDirectionBusStop>()
                .HasOne(e => e.Stop)
                .WithMany(s => s.RouteDirectionBusStops)
                .HasForeignKey(x => x.BusStopId);

            #endregion

            modelBuilder.Entity<FrequentLocation>()
                .Property(l => l.CreatedAt)
                .ForNpgsqlHasDefaultValueSql("NOW()");
        }
    }
}
