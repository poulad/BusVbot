using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Framework;
using Framework.Extensions;
using Moq;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using TelegramTests.Shared;
using Xunit;

namespace TelegramTests
{
    [Collection("/start command")]
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

            // should send the start instructions
            string text = "Hello Alice!\n" +
                          "BusV bot is at your service. ☺\n\n" +
                          "Try /help command to get some info.";
            _fixture.MockBotClient
                .Setup(botClient => botClient.MakeRequestAsync(
                    Is.SameJson<SendMessageRequest>($@"{{ chat_id: 333, text: ""{text.Stringify()}"" }}"),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Empty(responseContent);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }
    }
}
