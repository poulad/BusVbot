//using System.Linq;
//using System.Threading.Tasks;
//using BusV.Telegram.Models.Cache;
//using BusV.Telegram.Services;
//using Telegram.Bot.Framework.Abstractions;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.ReplyMarkups;
//
//namespace BusV.Telegram.Handlers.Commands
//{
//    public class SaveCommand : CommandBase
//    {
//        private readonly ILocationsManager _locationsManager;
//
//        public SaveCommand(
//            ILocationsManager locationsManager
//        )
//        {
//            _locationsManager = locationsManager;
//        }
//
//        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
//        {
//            string locationName = string.Join(' ', args.Skip(1));
//            Location location;
//
//            if (context.Update.Message.ReplyToMessage?.Location != null)
//            {
//                location = context.Update.Message.ReplyToMessage.Location;
//            }
//            else if (!string.IsNullOrWhiteSpace(context.Update.Message.ReplyToMessage?.Text))
//            {
//                string text = context.Update.Message.ReplyToMessage.Text;
//                location = _locationsManager.TryParseLocation(text).Location;
//            }
//            else
//            {
//                location = null;
//            }
//
//            bool areArgsValid = !string.IsNullOrWhiteSpace(locationName) && location != null;
//            //                   (Location != null ^ BusCommandArgs.IsValid);
//            // ToDo either location or bus args is valid for each /save command bcuz u reply to either a location or a /bus command call
//
//            if (!areArgsValid)
//            {
//                await context.Bot.Client.SendTextMessageAsync(
//                    context.Update.Message.Chat.Id, Constants.SaveCommandHelpMessage,
//                    ParseMode.Markdown,
//                    replyToMessageId: context.Update.Message.MessageId
//                ).ConfigureAwait(false);
//
//                return;
//            }
//
//            var uc = (UserChat)context.Update;
//
//            var locationsCount = await _locationsManager.FrequentLocationsCountAsync(uc)
//                .ConfigureAwait(false);
//
//            if (locationsCount < Telegram.Constants.Location.MaxSavedLocations)
//            {
//                var closeLocationTuple = await _locationsManager.TryFindSavedLocationCloseToAsync(uc, location)
//                    .ConfigureAwait(false);
//                if (closeLocationTuple.Exists)
//                {
//                    await context.Bot.Client.SendTextMessageAsync(
//                        context.Update.Message.Chat.Id,
//                        string.Format(Constants.LocationExistsMessageFormat, closeLocationTuple.Location.Name),
//                        ParseMode.Markdown,
//                        replyToMessageId: context.Update.Message.MessageId
//                    ).ConfigureAwait(false);
//                }
//                else
//                {
//                    await _locationsManager.PersistFrequentLocationAsync(uc, location, locationName)
//                        .ConfigureAwait(false);
//
//                    await context.Bot.Client.SendTextMessageAsync(
//                        context.Update.Message.Chat.Id,
//                        string.Format(Constants.LocationSavedMessageFormat, locationName),
//                        ParseMode.Markdown,
//                        replyToMessageId: context.Update.Message.MessageId,
//                        replyMarkup: new ReplyKeyboardRemove()
//                    ).ConfigureAwait(false);
//                }
//            }
//            else
//            {
//                await context.Bot.Client.SendTextMessageAsync(
//                    context.Update.Message.Chat.Id,
//                    Constants.MaxSaveLocationReachedMessage,
//                    replyToMessageId: context.Update.Message.MessageId,
//                    replyMarkup: new ReplyKeyboardRemove()
//                ).ConfigureAwait(false);
//            }
//        }
//
//        private static class Constants
//        {
//            public const string SaveCommandHelpMessage =
//                "_Wrong usage of save command_\n" +
//                "You can save up to 4 locations you frequently take bus from.\n\n" +
//                "Use it such as:\n" +
//                "`/save Home`\n" +
//                "while replying to a location message.";
//
//            public const string LocationSavedMessageFormat =
//                "Got it! Frequent location saved as:\n_{0}_";
//
//            public const string LocationExistsMessageFormat =
//                "That location ☝ is very close to your other frequent location:\n_{0}_";
//
//            public static readonly string MaxSaveLocationReachedMessage =
//                $"I can't remember more than {Telegram.Constants.Location.MaxSavedLocations} locations 🙄.\n\n" +
//                "Try removing a saved location with /del ❌ command.";
//        }
//    }
//}
