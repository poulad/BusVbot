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
    public class StartCommandTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public StartCommandTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [OrderedFact(DisplayName = "Should reply with the start instructions")]
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

            _fixture.MockBotClient
                .Setup(botClient => botClient.SendTextMessageAsync(
                    /* chatId: */ Is.SameJson<ChatId>("333"),
                    /* text: */ "Hello Alice!\n" +
                                "BusV bot is at your service ☺\n\n" +
                                "Try /help command to get some info",
                    ParseMode.Markdown,
                    default, default, default, default, default
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }
    }
}
