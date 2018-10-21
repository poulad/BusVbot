using BusV.Telegram.Models;
using Microsoft.EntityFrameworkCore;

namespace BusV.Telegram.Tests.Helpers
{
    internal class DbContextProvider
    {
        internal static BusVbotDbContext CreateInMemoryDbContext(string dbName = "", bool deleteIfExists = true)
        {
            var options = new DbContextOptionsBuilder<BusVbotDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var dbContext = new BusVbotDbContext(options);

            if (deleteIfExists)
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            }

            return dbContext;
        }
    }
}