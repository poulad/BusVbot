using System.Threading.Tasks;
using BusV.Telegram;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot;

namespace TelegramTests.Shared
{
    public class WebAppFactory : WebApplicationFactory<Startup>
    {
        public Mock<ITelegramBotClient> MockBotClient { get; set; }

        public WebAppFactory()
        {
            MockBotClient = new Mock<ITelegramBotClient>();

            // Skip setting the webhook
            MockBotClient
                .Setup(client =>
                    client.SetWebhookAsync(It.IsAny<string>(), default, default, default, default)
                )
                .Returns(Task.CompletedTask);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(ConfigureServices);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<BusVbot, MockBot>(_ => new MockBot(MockBotClient));
        }
    }
}
