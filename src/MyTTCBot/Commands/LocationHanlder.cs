using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyTTCBot.Models;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public interface ILocationHandler : IMessageHandler
    {

    }

    public class LocationHanlder : ILocationHandler
    {
        public const string LocationRegex = @"geo:([+|-]?\d+(?:.\d+)?),([+|-]?\d+(?:.\d+)?)";

        private readonly IMemoryCache _cache;

        public LocationHanlder(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task HandleMessage(Message message)
        {
            var userChat = new UserChat(message.From.Id, message.Chat.Id);
            UserContext context;
            if (!_cache.TryGetValue(userChat, out context))
            {
                context = new UserContext
                {
                    Location = new UserLocation()
                };
            }

            if (message.Location != null)
            {
                context.Location = (UserLocation)message.Location;
            }
            else
            {
                var match = Regex.Match(message.Text, LocationRegex, RegexOptions.IgnoreCase);
                context.Location = new UserLocation
                {
                    Latitude = double.Parse(match.Groups[1].Value),
                    Longitude = double.Parse(match.Groups[2].Value),
                };
            }

            _cache.Set(userChat, context);
            return Task.CompletedTask;
        }
    }
}
