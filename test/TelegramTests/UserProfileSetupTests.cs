using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Framework;
using Framework.Assertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramTests.Shared;
using Xunit;

namespace TelegramTests
{
    [Collection("user profile setup")]
    public class UserProfileSetupTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public UserProfileSetupTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [OrderedFact(DisplayName = "Should reply with the initial profile setup instructions")]
        public async Task Should_Send_Profile_Setup_Instructions()
        {
            string update = @"{
                update_id: 1,
                message: {
                    message_id: 2,
                    text: ""let's get started"",
                    chat: {
                        id: 1234,
                        type: ""private""
                    },
                    from: {
                        id: 1234,
                        first_name: ""Poulad"",
                        is_bot: false
                    },
                    date: 1000
                }
            }";

            // mock calls to the bot client
            {
                // first message with the country inline buttons
                _fixture.MockBotClient
                    .Setup(botClient => botClient.SendTextMessageAsync(
                        It.Is<ChatId>(id => id == "1234"),
                        "Select a country and then a region to find your local transit agency",
                        default, default, default, default,
                        It.Is(Asserts.JsonEqual(
                            new InlineKeyboardMarkup(new[]
                            {
                                new[] { InlineKeyboardButton.WithCallbackData("Canada 🇨🇦", "ups/c:Canada") },
                                new[] { InlineKeyboardButton.WithCallbackData("USA 🇺🇸", "ups/c:USA") },
                            })
                        )),
                        default
                    ))
                    .ReturnsAsync(null as Message);

                // second message for sharing the location
                _fixture.MockBotClient
                    .Setup(botClient => botClient.SendTextMessageAsync(
                        It.Is<ChatId>(id => id == "1234"),
                        "or *Share your location* so I can find it for you",
                        ParseMode.Markdown,
                        default, default, default,
                        It.Is(Asserts.JsonEqual(
                            new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton("Share my location") { RequestLocation = true },
                            }, true, true)
                        )),
                        default
                    ))
                    .ReturnsAsync(null as Message);
            }

            // ToDo ensure user profile is removed
            // IUserProfileRepo profileRepo = _factory.Server.Host.Services.GetRequiredService<IUserProfileRepo>();
            // UserProfile a = await profileRepo.GetByUserchatAsync("1234", "1234");

            // ensure cache is clear
            await _fixture.Cache.RemoveAsync("{\"u\":1234,\"c\":1234}");

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();

            string cachedContext = await _fixture.Cache.GetStringAsync("{\"u\":1234,\"c\":1234}");
            Asserts.JsonEqual(
                "{\"ProfileSetup\":{\"IsInstructionsSent\":true}}",
                cachedContext
            );
        }
    }
}
