using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using Framework;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
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

            // ensure user profile does not exist in the db
            IUserProfileRepo userRepo = _fixture.Services.GetRequiredService<IUserProfileRepo>();
            await userRepo.DeleteAsync("1234", "1234");

            // ensure cache is clear
            await _fixture.Cache.RemoveAsync(@"{""u"":1234,""c"":1234,""k"":""profile""}");

            // should send the first message with the country inline buttons
            _fixture.MockBotClient
                .Setup(botClient => botClient.MakeRequestAsync(
                    Is.SameJson<SendMessageRequest>($@"{{
                        chat_id: 1234,
                        text: ""Select a country and then a region to find your local transit agency"",
                        reply_markup: {{
                            inline_keyboard: [
                                [ {{ text: ""🇨🇦 Canada"", callback_data: ""ups/c:Canada"" }} ],
                                [ {{ text: ""🇺🇸 USA"", callback_data: ""ups/c:USA"" }} ],
                                [ {{ text: ""🏁 Test"", callback_data: ""ups/c:Test"" }} ]
                            ]
                        }}
                    }}"),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new Message { MessageId = 3 });

            // should send the second message for sharing the location
            _fixture.MockBotClient
                .Setup(botClient => botClient.MakeRequestAsync(
                    Is.SameJson<SendMessageRequest>($@"{{
                        chat_id: 1234,
                        text: ""or *Share your location* so I can find it for you"",
                        parse_mode: ""Markdown"",
                        reply_markup: {{
                            keyboard: [ [ {{ text: ""Share my location"", request_location: true }} ] ],
                            resize_keyboard: true,
                            one_time_keyboard: true
                        }}
                    }}"),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new Message { MessageId = 4 });

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Empty(responseContent);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();

            string cachedContext = await _fixture.Cache.GetStringAsync(@"{""u"":1234,""c"":1234,""k"":""profile""}");
            Asserts.JsonEqual(
                @"{""instructions_sent"":true,""agency_selection_msg"":3,""location_msg"":4}",
                cachedContext
            );
        }
    }
}
