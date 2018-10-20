//using System.IO;
//using Microsoft.Extensions.Configuration;

//namespace BusVbot.Tests.Helpers
//{
//    public static class ConfigurationProvider
//    {
//        private static readonly IConfigurationRoot Configuration;

//        static ConfigurationProvider()
//        {
//            Configuration = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json")
//                .AddJsonFile("appsettings.Development.json", true)
//                .Build();
//        }

//        public static class TelegramBot
//        {
//            public static string ApiToken => Configuration.GetValue<string>("TelegramBot:ApiToken");
//        }
//    }
//}
