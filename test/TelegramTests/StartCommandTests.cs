using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Framework;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramTests.Shared;
using Xunit;

namespace TelegramTests
{
    [Collection("start command")]
    public class StartCommandTests : IClassFixture<WebAppFactory>
    {
        private readonly WebAppFactory _factory;

        public StartCommandTests(WebAppFactory factory)
        {
            _factory = factory;
        }

        [OrderedFact(DisplayName = "Should reply with start instructions")]
        public async Task Should_Reply_Start_Instructions()
        {
            string update = @"{
                update_id: 1,
                message: {
                    message_id: 2,
                    text: ""/start"",
                    chat: {
                        id: 333,
                        type: ""private""
                    },
                    from: {
                        id: 333,
                        first_name: ""Alice"",
                        is_bot: false
                    },
                    entities: [ { offset: 0, length: 6, type: ""bot_command"" } ],
                    date: 1000
                }
            }";

            _factory.MockBotClient
                .Setup(botClient => botClient.SendTextMessageAsync(
                    It.Is<ChatId>(id => id == "333"),
                    "Hello Alice!\n" +
                    "BusV bot is at your service ☺\n\n" +
                    "Try /help command to get some info",
                    ParseMode.Markdown,
                    default, default, default, default, default
                ))
                .ReturnsAsync(null as Message);

            HttpClient client = _factory.CreateClient();
            HttpResponseMessage response = await client.PostWebhookUpdateAsync(update);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _factory.MockBotClient.VerifyAll();
            _factory.MockBotClient.VerifyNoOtherCalls();
        }
    }
}
