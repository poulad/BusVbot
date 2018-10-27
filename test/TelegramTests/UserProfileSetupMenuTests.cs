using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BusV.Data;
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
    [Collection("user profile setup menu")]
    public class UserProfileSetupMenuTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public UserProfileSetupMenuTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [OrderedFact(DisplayName = "Should update the menu with Ontario region for Canada")]
        public async Task Should_Update_With_Ontario_Region()
        {
            // ToDo remove msg date: https://github.com/TelegramBots/Telegram.Bot/issues/801
            // for the message in a callback query update, message content and message date will not be available
            // if the message is too old.
            string update = @"{
                update_id: 1,
                callback_query: {
                    id: ""5399"",
                    data: ""ups/c:canada"",
                    from: {
                        id: 1234,
                        first_name: ""Poulad"",
                        is_bot: false
                    },
                    message: {
                        message_id: 2744,
                        date: 200,
                        from: {
                            id: 1234,
                            first_name: ""Poulad"",
                            is_bot: false
                        },
                        chat: {
                            id: 1234,
                            type: ""private""
                        }
                    },
                    chat_instance: ""-9999"",
                    date: 1100
                }
            }";

            // ensure cache is set
            await _fixture.Cache.SetStringAsync(
                @"{""u"":1234,""c"":1234}",
                @"{""ProfileSetup"":{""IsInstructionsSent"":true}}"
            );

            // mock updating the menu
            _fixture.MockBotClient
                .Setup(botClient => botClient.EditMessageReplyMarkupAsync(
                    /* chatId: */ Is.SameJson<ChatId>("1234"),
                    /* messageId: */ 2744,
                    /* replyMarkup: */ Is.SameJson<InlineKeyboardMarkup>(@"{
                        inline_keyboard: [
                            [{ text: ""🌐 Back to countries"", callback_data: ""ups/c"" }],
                            [{ text: ""Ontario"", callback_data: ""ups/r:Ontario"" }]
                        ]
                    }"),
                    default
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // responds to the webhook with a bot API request
            string responseContent = await response.Content.ReadAsStringAsync();
            Asserts.IsJson(responseContent);
            Asserts.JsonEqual(@"{
                    callback_query_id: ""5399"",
                    method: ""answerCallbackQuery""
                }",
                responseContent
            );

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }

        [OrderedFact(DisplayName = "Should update the menu with TTC agency for Ontario region")]
        public async Task Should_Update_With_TTC_Agency()
        {
            // ToDo remove msg date: https://github.com/TelegramBots/Telegram.Bot/issues/801
            // for the message in a callback query update, message content and message date will not be available
            // if the message is too old.
            string update = @"{
                update_id: 1,
                callback_query: {
                    id: ""5400"",
                    data: ""ups/r:ontario"",
                    from: {
                        id: 1234,
                        first_name: ""Poulad"",
                        is_bot: false
                    },
                    message: {
                        message_id: 2744,
                        date: 200,
                        from: {
                            id: 1234,
                            first_name: ""Poulad"",
                            is_bot: false
                        },
                        chat: {
                            id: 1234,
                            type: ""private""
                        }
                    },
                    chat_instance: ""-9999"",
                    date: 1100
                }
            }";

            // ensure cache is set
            await _fixture.Cache.SetStringAsync(
                @"{""u"":1234,""c"":1234}",
                @"{""ProfileSetup"":{""IsInstructionsSent"":true}}"
            );

            // mock updating the menu
            _fixture.MockBotClient
                .Setup(botClient => botClient.EditMessageReplyMarkupAsync(
                    /* chatId: */ Is.SameJson<ChatId>("1234"),
                    /* messageId: */ 2744,
                    /* replyMarkup: */ Is.SameJson<InlineKeyboardMarkup>(@"{
                        inline_keyboard: [
                            [{ text: ""🌐 Back to countries"", callback_data: ""ups/c"" }],
                            [{ text: ""Toronto TTC"", callback_data: ""ups/a:ttc"" }]
                        ]
                    }"),
                    default
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // responds to the webhook with a bot API request
            string responseContent = await response.Content.ReadAsStringAsync();
            Asserts.IsJson(responseContent);
            Asserts.JsonEqual(@"{
                    callback_query_id: ""5400"",
                    method: ""answerCallbackQuery""
                }",
                responseContent
            );

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }

        [OrderedFact(DisplayName = "Should set user's agency to TTC")]
        public async Task Should_Set_User_Agency_TTC()
        {
            // ToDo remove msg date: https://github.com/TelegramBots/Telegram.Bot/issues/801
            // for the message in a callback query update, message content and message date will not be available
            // if the message is too old.
            string update = @"{
                update_id: 1,
                callback_query: {
                    id: ""5400"",
                    data: ""ups/a:Ttc"",
                    from: {
                        id: 1234,
                        first_name: ""Poulad"",
                        is_bot: false
                    },
                    message: {
                        message_id: 2744,
                        date: 200,
                        from: {
                            id: 1234,
                            first_name: ""Poulad"",
                            is_bot: false
                        },
                        chat: {
                            id: 1234,
                            type: ""private""
                        }
                    },
                    chat_instance: ""-9999"",
                    date: 1100
                }
            }";

            // ensure user profile does not exist in the db
            IUserProfileRepo userRepo = _fixture.Services.GetRequiredService<IUserProfileRepo>();
            await userRepo.DeleteAsync("1234", "1234");

            // ensure cache is set
            await _fixture.Cache.SetStringAsync(
                @"{""u"":1234,""c"":1234}",
                @"{""ProfileSetup"":{""IsInstructionsSent"":true}}"
            );

            // mock acknowledging that the new agency is set
            _fixture.MockBotClient
                .Setup(botClient => botClient.SendTextMessageAsync(
                    /* chatId: */ Is.SameJson<ChatId>("1234"),
                    /* text: */ "Great! Your default agency is now set to *Toronto Transit Commission* " +
                                "in Ontario, Canada.\n\n\n" +
                                "💡 *Pro Tip*: You can always view or modify it using the /profile command",
                    /* parseMode: */ ParseMode.Markdown,
                    default, default, default,
                    /* replyMarkup: */ Is.SameJson<IReplyMarkup>(@"{ remove_keyboard: true }"),
                    default
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // responds to the webhook with a bot API request
            string responseContent = await response.Content.ReadAsStringAsync();
            Asserts.IsJson(responseContent);
            Asserts.JsonEqual(@"{
                    chat_id: 1234,
                    message_id: 2744,
                    method: ""editMessageReplyMarkup""
                }",
                responseContent
            );

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();
        }
    }
}
