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
using TelegramTests.Shared;
using Xunit;

namespace TelegramTests
{
    [Collection("profile removal")]
    public class ProfileRemovalTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public ProfileRemovalTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [OrderedFact(DisplayName = "Should remove user profile upon confirmation")]
        public async Task Should_Remove_Profile()
        {
            // user sends a new text message with the text "forget me"
            // replying to the profile removal prompt message from the BusVbot
            string update = @"{
                update_id: 1,
                message: {
                    message_id: 22,
                    text: ""forget me"",
                    chat: {
                        id: 789,
                        type: ""private""
                    },
                    from: {
                        id: 789,
                        first_name: ""Jack"",
                        is_bot: false
                    },
                    reply_to_message: {
                        message_id: 18,
                        text: ""Are you sure you want to remove your profile?"",
                        chat: {
                            id: 789,
                            type: ""private""
                        },
                        from: {
                            id: 420,
                            first_name: ""BusVbot"",
                            is_bot: true,
                            username: ""busvbot""
                        },
                        date: 2568
                    },
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

            // ensure some value is set in the cache
            await _fixture.Cache.SetStringAsync(@"{""u"":1234,""c"":1234}", "{}");

            // should send a message acknowledging the profile removal
            _fixture.MockBotClient
                .Setup(botClient => botClient.SendTextMessageAsync(
                    /* chatId: */ Is.SameJson<ChatId>("789"),
                    /* text: */ "Your _profile is now removed_ but this doesn't need to be a goodbye! 😉\n\n" +
                                "Come back whenever you needed my services 🚍🏃 and we can start over again.",
                    /* parseMode: */ ParseMode.Markdown,
                    default, default, default, default,
                    /* cancellationToken: */ It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();

            // responds to the webhook with a bot API request
            // should delete the profile removal prompt message that bot sent earlier
            string responseContent = await response.Content.ReadAsStringAsync();
            Asserts.IsJson(responseContent);
            Asserts.JsonEqual(@"{
                    chat_id: 789,
                    message_id: 18,
                    method: ""deleteMessage""
                }",
                responseContent
            );

            // cache for that userchat should be removed
            string cachedContext = await _fixture.Cache.GetStringAsync(@"{""u"":789,""c"":789}");
            Assert.Null(cachedContext);

            // user profile should be removed from the database
            UserProfile userProfile = await userRepo.GetByUserchatAsync("789", "789");
            Assert.Null(userProfile);
        }

        [OrderedFact(DisplayName = "Should ignore invalid reply to the profile removal prompt message")]
        public async Task Should_Ignore_Invalid_Reply()
        {
            string update = @"{
                update_id: 1,
                message: {
                    message_id: 22,
                    text: ""forget nobody"",
                    chat: {
                        id: 789,
                        type: ""private""
                    },
                    from: {
                        id: 789,
                        first_name: ""Jack"",
                        is_bot: false
                    },
                    reply_to_message: {
                        message_id: 18,
                        text: ""Are you sure you want to remove your profile?"",
                        chat: {
                            id: 789,
                            type: ""private""
                        },
                        from: {
                            id: 420,
                            first_name: ""BusVbot"",
                            is_bot: true,
                            username: ""busvbot""
                        },
                        date: 2568
                    },
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

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();

            // responds to the webhook with a bot API request
            // should send a "typing" chat action
            string responseContent = await response.Content.ReadAsStringAsync();
            Asserts.IsJson(responseContent);
            Asserts.JsonEqual(@"{
                    chat_id: 789,
                    action: ""typing"",
                    method: ""sendChatAction""
                }",
                responseContent
            );

            // user profile should remain as is
            UserProfile userProfile = await userRepo.GetByUserchatAsync("789", "789");
            Assert.NotNull(userProfile);
        }
    }
}
