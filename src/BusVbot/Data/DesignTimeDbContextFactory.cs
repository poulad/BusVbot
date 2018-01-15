using System;
using BusVbot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BusVbot.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BusVbotDbContext>
    {
        public BusVbotDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<BusVbotDbContext>();
            var configuration = Startup.BuildConfiguration(AppDomain.CurrentDomain.BaseDirectory).Build();

            builder
                .UseNpgsql(configuration["ConnectionString"])
                .EnableSensitiveDataLogging();
            return new BusVbotDbContext(builder.Options);
        }
    }
}