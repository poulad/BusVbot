using System.Threading.Tasks;
using BusVbot.Bot;
using BusVbot.Models.Cache;
using BusVbot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusVbot.Handlers.Commands
{
    public class SaveCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }

        public string Name { get; set; }

        public Location Location { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Name) &&
                               Location != null;

        //                   (Location != null ^ BusCommandArgs.IsValid); 
        // ToDo either location or bus args is valid for each /save command bcuz u reply to either a location or a /bus command call
    }

    public class SaveCommand : CommandBase<SaveCommandArgs>
    {
        private readonly ILocationsManager _locationsManager;

        public SaveCommand(ILocationsManager locationsManager)
            : base(Constants.CommandName)
        {
            _locationsManager = locationsManager;
        }

        protected override SaveCommandArgs ParseInput(Update update)
        {
            var args = base.ParseInput(update);

            args.Name = args.ArgsInput;

            if (update.Message.ReplyToMessage?.Location != null)
            {
                args.Location = update.Message.ReplyToMessage.Location;
            }
            else if (!string.IsNullOrWhiteSpace(update.Message.ReplyToMessage?.Text))
            {
                var text = update.Message.ReplyToMessage.Text;
                var loc = _locationsManager.TryParseLocation(text);
                if (loc.Successful)
                {
                    args.Location = loc.Location;
                }
            }

            return args;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, SaveCommandArgs args)
        {
            if (!args.IsValid)
            {
                await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id, Constants.SaveCommandHelpMessage,
                    ParseMode.Markdown,
                    replyToMessageId: update.Message.MessageId);

                return UpdateHandlingResult.Handled;
            }

            var uc = (UserChat) update;

            var locationsCount = await _locationsManager.FrequentLocationsCountAsync(uc);

            if (locationsCount < CommonConstants.Location.MaxSavedLocations)
            {
                var closeLocationTuple = await _locationsManager.TryFindSavedLocationCloseToAsync(uc, args.Location);
                if (closeLocationTuple.Exists)
                {
                    await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id,
                        string.Format(Constants.LocationExistsMessageFormat, closeLocationTuple.Location.Name),
                        ParseMode.Markdown,
                        replyToMessageId: update.Message.MessageId);
                }
                else
                {
                    await _locationsManager.PersistFrequentLocationAsync(uc, args.Location, args.Name);

                    await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id,
                        string.Format(Constants.LocationSavedMessageFormat, args.Name),
                        ParseMode.Markdown,
                        replyToMessageId: update.Message.MessageId,
                        replyMarkup: new ReplyKeyboardRemove());
                }
            }
            else
            {
                await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id,
                    Constants.MaxSaveLocationReachedMessage,
                    replyToMessageId: update.Message.MessageId,
                    replyMarkup: new ReplyKeyboardRemove());
            }
            return UpdateHandlingResult.Handled;
        }

        private static class Constants
        {
            public const string CommandName = "save";

            public const string SaveCommandHelpMessage =
                "_Wrong usage of save command_\n" +
                "You can save up to 4 locations you frequently take bus from.\n\n" +
                "Use it such as:\n" +
                "`/save Home`\n" +
                "while replying to a location message.";

            public const string LocationSavedMessageFormat =
                "Got it! Frequent location saved as:\n_{0}_";

            public const string LocationExistsMessageFormat =
                "That location ☝ is very close to your other frequent location:\n_{0}_";

            public static readonly string MaxSaveLocationReachedMessage =
                $"I can't remember more than {CommonConstants.Location.MaxSavedLocations} locations 🙄.\n\n" +
                "Try removing a saved location with /del ❌ command.";
        }
    }
}