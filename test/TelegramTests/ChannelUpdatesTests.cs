using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Framework;
using TelegramTests.Shared;
using Xunit;

namespace TelegramTests
{
    [Collection("channel updates")]
    public class ChannelUpdatesTests : IClassFixture<WebAppFactory>
    {
        private readonly WebAppFactory _factory;

        public ChannelUpdatesTests(WebAppFactory factory)
        {
            _factory = factory;
        }

        [OrderedFact(DisplayName = "Should ignore channel posts")]
        public async Task Should_Ignore_Posts()
        {
            string update = @"{
                ""update_id"": 5952,
                ""channel_post"": {
                    ""message_id"": 760,
                    ""chat"": {
                        ""id"": -101,
                        ""title"": ""My Channel"",
                        ""type"": ""channel""
                    },
                    ""date"": 1540000000,
                    ""text"": ""Hola Mundo""
                }
            }";

            HttpClient client = _factory.CreateClient();
            HttpResponseMessage response = await client.PostWebhookUpdateAsync(update);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _factory.MockBotClient.VerifyAll();
            _factory.MockBotClient.VerifyNoOtherCalls();
        }
    }
}
