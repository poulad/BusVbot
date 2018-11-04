using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using Framework;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramTests.Shared;
using Xunit;

namespace TelegramTests
{
    [Collection("/bus command")]
    public class BusCommandTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public BusCommandTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [OrderedFact(DisplayName =
            "Should show the bus command instructions and 2 TTC agency routes for an empty \"/bus\" command"
        )]
        public async Task Should_Show_Bus_Instructions_And_Routes()
        {
            string update = @"{
                update_id: 1,
                message: {
                    message_id: 2,
                    text: ""/bus"",
                    chat: {
                        id: 789,
                        type: ""private""
                    },
                    from: {
                        id: 789,
                        first_name: ""Jack"",
                        is_bot: false
                    },
                    entities: [ { offset: 0, length: 4, type: ""bot_command"" } ],
                    date: 2680
                }
            }";

            // ensure user profile is persisted in the db
            IUserProfileRepo userRepo = _fixture.Services.GetRequiredService<IUserProfileRepo>();
            await userRepo.DeleteAsync("789", "789");
            await userRepo.AddAsync(new UserProfile
            {
                ChatId = "789",
                UserId = "789",
                DefaultAgencyTag = "ttc"
            });

            // should send the instructions message
            _fixture.MockBotClient
                .Setup(botClient => botClient.SendTextMessageAsync(
                    /* chatId: */ Is.SameJson<ChatId>("789"),
                    /* text: */ "This is not enough information.\n" +
                                "You can check for a bus like the _6 Bay St. Southbound_ in any of these formats:\n" +
                                "```\n" +
                                "/bus 6\n" +
                                "/bus 6 southbound\n" +
                                "/bus 6 south\n" +
                                "/bus 6 s\n" +
                                "```\n" +
                                "There are 2 routes.\n\n" +
                                "6-Bay\n" +
                                "34-Eglinton East",
                    /* parseMode: */ ParseMode.Markdown,
                    default, default,
                    /* replyToMessageId: */ 2,
                    default,
                    /* cancellationToken: */ It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }
    }
}
