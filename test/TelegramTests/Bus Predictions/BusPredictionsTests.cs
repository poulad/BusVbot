using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Data.Entities;
using Framework;
using Framework.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using TelegramTests.Shared;
using Xunit;

namespace TelegramTests
{
    /// <summary>
    /// Tests for <see cref="BusV.Telegram.Handlers.LocationHandler"/>
    /// and <see cref="BusV.Telegram.Handlers.BusPredictionsHandler"/>
    /// </summary>
    [Collection("bus predictions")]
    public class BusPredictionsTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public BusPredictionsTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [OrderedFact(DisplayName = "Should cache the location from a location message and ask for the bus route")]
        public async Task Should_Cache_Location_Ask_For_Route()
        {
            // a location message
            string update = @"{
                update_id: 27,
                message: {
                    message_id: 45,
                    location: { latitude: 43.6606, longitude: -79.3852 },
                    chat: { id: 83, type: ""private"" },
                    from: { id: 83, first_name: ""Jack"", is_bot: false },
                    date: 46862680
                }
            }";

            // ensure user profile is persisted in the db
            IUserProfileRepo userRepo = _fixture.Services.GetRequiredService<IUserProfileRepo>();
            await userRepo.DeleteAsync("83", "83");
            await userRepo.AddAsync(new UserProfile
            {
                ChatId = "83",
                UserId = "83",
                DefaultAgencyTag = "ttc"
            });

            // ensure cache has no bus
            await _fixture.Cache.RemoveAsync(@"{""u"":83,""c"":83,""k"":""bus""}");

            // ensure cache has no location
            await _fixture.Cache.RemoveAsync(@"{""u"":83,""c"":83,""k"":""location""}");

            // should ask to choose the bus
            string text = "There you are! What's the bus you want to catch?\n" +
                          "Send me using 👉 /bus command.";
            _fixture.MockBotClient
                .Setup(botClient => botClient.MakeRequestAsync(
                    Is.SameJson<SendMessageRequest>($@"{{
                        chat_id: 83,
                        text: ""{text.Stringify()}"",
                        reply_to_message_id: 45,
                        reply_markup: {{ remove_keyboard: true }}
                    }}"),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(null as Message);

            HttpResponseMessage response = await _fixture.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Empty(responseContent);

            _fixture.MockBotClient.VerifyAll();
            _fixture.MockBotClient.VerifyNoOtherCalls();

            // check whether location is cached
            string locationContext = await _fixture.Cache.GetStringAsync(@"{""u"":83,""c"":83,""k"":""location""}");
            Asserts.JsonEqual("{ location_msg: 45, lat: 43.6606, lon: -79.3852 }", locationContext);
        }
    }
}
