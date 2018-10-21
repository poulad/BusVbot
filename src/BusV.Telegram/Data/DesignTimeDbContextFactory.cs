using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using BusV.Telegram.Models;

namespace BusV.Telegram.Data
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