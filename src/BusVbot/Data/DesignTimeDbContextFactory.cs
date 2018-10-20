using BusVbot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;

namespace BusVbot.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BusVbotDbContext>
    {
        public BusVbotDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<BusVbotDbContext>();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddCommandLine(args)
                .AddJsonFile("appsettings.json")
                .AddJsonEnvVar("BUSVBOT_SETTINGS", true)
                .Build();

            builder
                .UseNpgsql(configuration["ConnectionString"])
                .EnableSensitiveDataLogging();
            return new BusVbotDbContext(builder.Options);
        }
    }
}