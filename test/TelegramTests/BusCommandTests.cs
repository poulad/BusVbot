using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using Framework;
using Framework.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
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
            string text = "This is not enough information.\n" +
                          "You can check for a bus like the _6 Bay St. Southbound_ in any of these formats:\n" +
                          "```\n" +
                          "/bus 6\n" +
                          "/bus 6 southbound\n" +
                          "/bus 6 south\n" +
                          "/bus 6 s\n" +
                          "```\n" +
                          "There are 2 routes.\n\n" +
                          "6-Bay\n" +
                          "34-Eglinton East";
            _fixture.MockBotClient
                .Setup(botClient => botClient.MakeRequestAsync(
                    Is.SameJson<SendMessageRequest>($@"{{
                        chat_id: 789,
                        text: ""{text.Stringify()}"",
                        parse_mode: ""Markdown"",
                        reply_to_message_id: 2,
                        reply_markup: {{ remove_keyboard: true }}
                    }}"),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }

        [OrderedFact(DisplayName = "Should ask for the direction to take for a \"/bus 6\" command")]
        public async Task Should_Ask_For_Direction()
        {
            // "/bus" command with a valid route but no direction
            string update = @"{
                update_id: 1,
                message: {
                    message_id: 2,
                    text: ""/bus 6"",
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

            // should send a message asking for a direction to choose
            _fixture.MockBotClient
                .Setup(botClient => botClient.MakeRequestAsync(
                    Is.SameJson<SendMessageRequest>(@"{
                        chat_id: 789,
                        text: ""6-Bay"",
                        reply_to_message_id: 2,
                        reply_markup: {
                            inline_keyboard: [
                                [ { text: ""Northbound"", callback_data: ""bus/r:6/d:North"" } ],
                                [ { text: ""Southbound"", callback_data: ""bus/r:6/d:South"" } ]
                            ]
                        }
                    }"),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }
    }
}
