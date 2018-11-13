using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Framework;
using Microsoft.Extensions.Caching.Distributed;
using TelegramTests.Shared;
using Xunit;

namespace TelegramTests
{
    [Collection("/cancel command")]
    public class CancelCommandTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fxt;

        public CancelCommandTests(TestsFixture fixture)
        {
            _fxt = fixture;
        }

        [OrderedFact("Should delete all the cached items")]
        public async Task Should_Remove_Cache()
        {
            string update = @"{
                update_id: 645,
                message: {
                    message_id: 352,
                    text: ""/cancel"",
                    chat: { id: 333, type: ""private"" },
                    from: { id: 333, first_name: ""John"", is_bot: false },
                    entities: [ { offset: 0, length: 7, type: ""bot_command"" } ],
                    date: 5641457
                }
            }";

            // ensure cache has values
            await _fxt.Cache.SetStringAsync(@"{""u"":333,""c"":333,""k"":""profile""}", "foo: 1");
            await _fxt.Cache.SetStringAsync(@"{""u"":333,""c"":333,""k"":""bus""}", "{}");
            await _fxt.Cache.SetStringAsync(@"{""u"":333,""c"":333,""k"":""location""}", "VALUE");

            HttpResponseMessage response = await _fxt.HttpClient.PostWebhookUpdateAsync(update);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            Asserts.JsonEqual(@"{
                    method: ""sendChatAction"",
                    chat_id: 333,
                    action: ""typing""
                }",
                responseContent
            );

            _fxt.MockBotClient.VerifyAll();
            _fxt.MockBotClient.VerifyNoOtherCalls();

            string cachedLocation = await _fxt.Cache.GetStringAsync(@"{""u"":333,""c"":333,""k"":""location""}");
            string cachedBus = await _fxt.Cache.GetStringAsync(@"{""u"":333,""c"":333,""k"":""bus""}");
            string cachedProfile = await _fxt.Cache.GetStringAsync(@"{""u"":333,""c"":333,""k"":""profile""}");

            Assert.Null(cachedLocation);
            Assert.Null(cachedBus);
            Assert.Null(cachedProfile);
        }
    }
}
