using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyTTCBot.Commands;
using MyTTCBot.Models;
using NetTelegramBotApi.Types;
using Xunit;

namespace MyTTCBot.Tests
{
    public class LocationHandlerTests
    {
        [Fact(DisplayName = "Extract geo location from text")]
        public async Task ShouldReadLocationFromRegex()
        {
            const double lat = 43.787174;
            const double lon = -79.18591;
            var msg = new Message
            {
                MessageId = 111,
                Text = $"Location: geo:{lat},{lon}?z=17\nhttp://osmand.net/go?lat=${lat}&lon={lon}&z=17",
                Chat = new Chat
                {
                    Id = 999,
                },
                From = new User
                {
                    Id = 555,
                },
            };
            var cache = new MemoryCache(new MemoryCacheOptions());
            ILocationHandler sut = new LocationHanlder(cache);

            // act
            await sut.HandleMessage(msg);

            var context = cache.Get<UserContext>(new UserChat(msg.From.Id, msg.Chat.Id));
            Assert.Equal(lat, context.Location.Latitude);
            Assert.Equal(lon, context.Location.Longitude);
        }
    }
}
