using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using Framework;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramTests.Shared;
using Xunit;

namespace TelegramTests
{
    [Collection("/profile command")]
    public class ProfileCommandTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public ProfileCommandTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [OrderedFact(DisplayName = "Should show user profile for a \"/profile\" command")]
        public async Task Should_Show_Profile()
        {
            string update = @"{
                update_id: 1,
                message: {
                    message_id: 2,
                    text: ""/profile"",
                    chat: {
                        id: 789,
                        type: ""private""
                    },
                    from: {
                        id: 789,
                        first_name: ""Jack"",
                        is_bot: false
                    },
                    entities: [ { offset: 0, length: 8, type: ""bot_command"" } ],
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
                DefaultAgencyTag = "lametro"
            });

            // should send the profile information message
            _fixture.MockBotClient
                .Setup(botClient => botClient.SendTextMessageAsync(
                    /* chatId: */ Is.SameJson<ChatId>("789"),
                    /* text: */ "Hey Jack\n" +
                                "Your default transit agency is set to *Los Angeles Metro* in " +
                                "California-Southern, USA.\n\n" +
                                "💡 *Pro Tip*: You can remove your profile from my memory by sending " +
                                "the `/profile remove` message.",
                    /* parseMode: */ ParseMode.Markdown,
                    default, default, default, default,
                    /* cancellationToken: */ It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }

        [OrderedFact(DisplayName = "Should prompt the user for profile removal confirmation")]
        public async Task Should_Send_Profile_Removal_Instructions()
        {
            string update = @"{
                update_id: 1,
                message: {
                    message_id: 2,
                    text: ""/profile remove"",
                    chat: {
                        id: 789,
                        type: ""private""
                    },
                    from: {
                        id: 789,
                        first_name: ""Jack"",
                        is_bot: false
                    },
                    entities: [ { offset: 0, length: 8, type: ""bot_command"" } ],
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
                DefaultAgencyTag = "lametro"
            });

            // should send the profile removal prompt message
            _fixture.MockBotClient
                .Setup(botClient => botClient.SendTextMessageAsync(
                    /* chatId: */ Is.SameJson<ChatId>("789"),
                    /* text: */ "Jack, You are about to remove your profile. " +
                                "Sadly, I won't remember much of our conversations after the removal.😟\n\n" +
                                "*Are you sure you want to remove your profile?* Reply to this message " +
                                "with the text `forget me`.",
                    /* parseMode: */ ParseMode.Markdown,
                    default, default, default,
                    /* replyMarkup: */ Is.SameJson<IReplyMarkup>(@"{ force_reply: true }"),
                    /* cancellationToken: */ It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }

        [OrderedFact(DisplayName = "Should ignore the command if user has no profile set")]
        public async Task Should_Ignore_Command_If_No_Profile()
        {
            string update = @"{
                update_id: 1,
                message: {
                    message_id: 2,
                    text: ""/profile"",
                    chat: {
                        id: 789,
                        type: ""private""
                    },
                    from: {
                        id: 789,
                        first_name: ""Jack"",
                        is_bot: false
                    },
                    entities: [ { offset: 0, length: 8, type: ""bot_command"" } ],
                    date: 2680
                }
            }";

            // ensure user profile does not exist in the db
            IUserProfileRepo userRepo = _fixture.Services.GetRequiredService<IUserProfileRepo>();
            await userRepo.DeleteAsync("789", "789");

            // ensure cache is clear
            await _fixture.Cache.RemoveAsync(@"{""u"":789,""c"":789}");

            // should send the first message with the country inline buttons
            _fixture.MockBotClient
                .Setup(botClient => botClient.SendTextMessageAsync(
                    /* chatId: */ Is.SameJson<ChatId>("789"),
                    /* text: */ "Select a country and then a region to find your local transit agency",
                    default, default, default, default,
                    /* replyMarkup: */
                    Is.SameJson<IReplyMarkup>(@"{
                        inline_keyboard: [
                            [{ text: ""🇨🇦 Canada"", callback_data: ""ups/c:Canada"" }],
                            [{ text: ""🇺🇸 USA"", callback_data: ""ups/c:USA"" }],
                            [{ text: ""🏁 Test"", callback_data: ""ups/c:Test"" }]
                        ]
                        }"),
                    default
                ))
                .ReturnsAsync(null as Message);

            // should send the second message for sharing the location
            _fixture.MockBotClient
                .Setup(botClient => botClient.SendTextMessageAsync(
                    /* chatId: */ Is.SameJson<ChatId>("789"),
                    /* text: */ "or *Share your location* so I can find it for you",
                    /* parseMode: */
                    ParseMode.Markdown,
                    default, default, default,
                    /* replyMarkup: */
                    Is.SameJson<IReplyMarkup>(@"{
                            keyboard: [
                                [{ text: ""Share my location"", request_location: true }]
                            ],
                            resize_keyboard: true,
                            one_time_keyboard: true
                        }"),
                    default
                ))
                .ReturnsAsync(null as Message);
            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();

            string cachedContext = await _fixture.Cache.GetStringAsync(@"{""u"":789,""c"":789}");
            Asserts.JsonEqual(@"{""ProfileSetup"":{""IsInstructionsSent"":true}}", cachedContext);
        }
    }
}
