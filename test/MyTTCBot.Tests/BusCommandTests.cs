using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using MyTTCBot.Models;
using MyTTCBot.Services;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using Xunit;

namespace MyTTCBot.Tests
{
    public class BusCommandTests
    {
        /*
        private Message _testMessage;

        public BusCommandTests()
        {
            _testMessage = new Message
            {
                MessageId = 111,
                Text = "/bus 97 s",
                Chat = new Chat
                {
                    Id = 999,
                },
                From = new User
                {
                    Id = 555,
                },
                Date = DateTimeOffset.Now,
            };
        }

        [Fact(DisplayName = "Ask user for location")]
        public async Task ShouldAskForUserLocation()
        {
            var mockBot = new Mock<IBotService>();
            var mockNextBus = new Mock<INextBusService>();
            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
            IBusCommand sut = new BusCommand(mockBot.Object, mockNextBus.Object, cache);

            // act
            await sut.HandleMessage(_testMessage);

            mockBot.Verify(x => x.MakeRequest(It.Is<SendMessage>(m =>
                m.Text == "Send your location"
                ))
            , Times.Once);
        }

        [Fact(DisplayName = "Use user location from cache")]
        public async Task ShouldGetUserContextFromCache()
        {
            var mockBot = new Mock<IBotService>();
            var mockNextBus = new Mock<INextBusService>();
            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
            var userChat = new UserChat(_testMessage.From.Id, _testMessage.Chat.Id);
            var context = new UserContext
            {
                Location = new UserLocation
                {
                    Longitude = -79.405,
                    Latitude = 43.744,
                }
            };
            cache.Set(userChat, context);
            var busStop = new BusStop
            {
                Id = "bus stop",
                Name = "Test Bus Stop",
                Latitude = 43.743,
                Longitude = -79.406,
            };
            mockNextBus.Setup(x => x.FindNearestBusStop(
                It.IsAny<string>(),
                It.IsAny<BusDirection>(),
                It.Is<double>(lon => lon.Equals(context.Location.Longitude)),
                It.Is<double>(lat => lat.Equals(context.Location.Latitude))
            )).ReturnsAsync(busStop);
            mockBot.Setup(x => x.MakeRequest(It.IsAny<SendLocation>()))
                .ReturnsAsync(_testMessage);
            IBusCommand sut = new BusCommand(mockBot.Object, mockNextBus.Object, cache);

            // act
            await sut.HandleMessage(_testMessage);

            mockNextBus.Verify(x => x.FindNearestBusStop(
                It.IsAny<string>(),
                It.IsAny<BusDirection>(),
                It.Is<double>(lon => lon.Equals(context.Location.Longitude)),
                It.Is<double>(lat => lat.Equals(context.Location.Latitude))
                ));
        }

        [Fact(DisplayName = "Send nearest bus stop location")]
        public async Task ShouldSendBusStopLocation()
        {
            const float busStopLatitude = 43.7F;
            const float busStopLongitude = -79.4F;
            var mockBot = new Mock<IBotService>();
            var mockNextBus = new Mock<INextBusService>();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var userChat = new UserChat(_testMessage.From.Id, _testMessage.Chat.Id);
            var context = new UserContext
            {
                Location = new UserLocation
                {
                    Latitude = 43.744,
                    Longitude = -79.405,
                }
            };
            cache.Set(userChat, context);
            mockNextBus.Setup(x => x.FindNearestBusStop(
                It.IsAny<string>(),
                It.IsAny<BusDirection>(),
                It.Is<double>(y => y.Equals(context.Location.Longitude)),
                It.Is<double>(y => y.Equals(context.Location.Latitude)))
            ).ReturnsAsync(new BusStop
            {
                Id = "ID",
                Latitude = busStopLatitude,
                Longitude = busStopLongitude,
                Name = "Bus Stop",
            });
            mockBot.Setup(x => x.MakeRequest(It.Is<SendLocation>(y =>
                y.ReplyToMessageId == _testMessage.MessageId &&
                y.Latitude.Equals(busStopLatitude) &&
                y.Longitude.Equals(busStopLongitude)
            ))).ReturnsAsync(new Message
            {
                MessageId = 123,
                Chat = new Chat
                {
                    Id = 234,
                }
            });

            IBusCommand sut = new BusCommand(mockBot.Object, mockNextBus.Object, cache);

            // act
            await sut.HandleMessage(_testMessage);

            mockNextBus.Verify(x => x.FindNearestBusStop(
                It.IsAny<string>(),
                It.IsAny<BusDirection>(),
                It.Is<double>(y => y.Equals(context.Location.Longitude)),
                It.Is<double>(y => y.Equals(context.Location.Latitude)))
            );

            mockBot.Verify(x => x.MakeRequest(It.Is<SendLocation>(y =>
                y.ReplyToMessageId == _testMessage.MessageId &&
                y.Latitude.Equals(busStopLatitude) &&
                y.Longitude.Equals(busStopLongitude)
            )));
        }
    */
    }
}
