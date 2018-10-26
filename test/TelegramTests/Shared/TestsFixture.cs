using System;
using System.Net.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot;

namespace TelegramTests.Shared
{
    public class TestsFixture
    {
        public HttpClient HttpClient { get; }

        public Mock<ITelegramBotClient> MockBotClient => WebAppFactory.MockBotClient;

        public IDistributedCache Cache => Services.GetRequiredService<IDistributedCache>();

        public IServiceProvider Services => WebAppFactory.Server.Host.Services;

        public WebAppFactory WebAppFactory { get; }

        public TestsFixture()
        {
            WebAppFactory = new WebAppFactory();
            HttpClient = WebAppFactory.CreateClient();
        }
    }
}
