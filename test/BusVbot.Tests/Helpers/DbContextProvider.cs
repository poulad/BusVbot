using BusVbot.Models;
using Microsoft.EntityFrameworkCore;

namespace BusVbot.Tests.Helpers
{
    internal class DbContextProvider
    {
        internal static BusVbotDbContext CreateInMemoryDbContext(string dbName = "")
        {
            var options = new DbContextOptionsBuilder<BusVbotDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var dbContext = new BusVbotDbContext(options);

            return dbContext;
        }
    }
}
