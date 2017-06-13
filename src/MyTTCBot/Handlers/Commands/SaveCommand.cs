using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyTTCBot.Bot;
using MyTTCBot.Models.Cache;
using NetTelegram.Bot.Framework;
using NetTelegram.Bot.Framework.Abstractions;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using MyTTCBot.Services;

namespace MyTTCBot.Handlers.Commands
{
    public class SaveCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string Name { get; set; }

        public Location Location { get; set; }

        public BusCommandArgs BusCommandArgs { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Name) &&
                                Location != null;
        //                   (Location != null ^ BusCommandArgs.IsValid); 
        // todo either location or bus args is valid for each /save command bcuz u reply to either a location or a /bus command call
    }

    public class SaveCommand : CommandBase<SaveCommandArgs>
    {
        private readonly ILocationsManager _locationsManager;

        private readonly IPredictionsManager _predictionsManager;

        public SaveCommand(ILocationsManager locationsManager, IPredictionsManager predictionsManager)
            : base(Constants.CommandName)
        {
            _locationsManager = locationsManager;
            _predictionsManager = predictionsManager;
        }

        protected override SaveCommandArgs ParseInput(Update update)
        {
            var args = base.ParseInput(update);

            var matches = Regex.Match(update.Message.Text, Constants.SaveCommandRegex);
            if (matches.Success)
            {
                args.Name = matches.Groups["name"].Value.Trim();
            }

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
                else
                {
                    args.BusCommandArgs = _predictionsManager.ParseBusCommandArgs(update.Message.Text);
                }
            }

            return args;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, SaveCommandArgs args)
        {
            if (!args.IsValid)
            {
                await Bot.MakeRequest(new SendMessage(update.Message.Chat.Id, Constants.SaveCommandHelpMessage)
                {
                    ReplyToMessageId = update.Message.MessageId,
                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                });
                return UpdateHandlingResult.Handled;
            }

            var uc = (UserChat)update;

            // todo ensure userchat exists in db

            var locationsCount = await _locationsManager.FrequentLocationsCount(uc);

            if (locationsCount < CommonConstants.Location.MaxSavedLocations)
            {
                await _locationsManager.SaveFrequentLocationToDatabase(uc, args.Location, args.Name);

                await Bot.MakeRequest(
                    new SendMessage(update.Message.Chat.Id, Constants.LocationSavedMessage)
                    {
                        ReplyToMessageId = update.Message.MessageId,
                        ReplyMarkup = new ReplyKeyboardRemove { RemoveKeyboard = true }
                    });
            }
            else
            {
                await Bot.MakeRequest(
                    new SendMessage(update.Message.Chat.Id, Constants.MaxSaveLocationReachedMessage)
                    {
                        ReplyToMessageId = update.Message.MessageId,
                        ReplyMarkup = new ReplyKeyboardRemove { RemoveKeyboard = true }
                    });
            }
            return UpdateHandlingResult.Handled;
        }

        private static class Constants
        {
            public const string CommandName = "save";

            public const string SaveCommandRegex = "^/" + CommandName + @"\s+(?<name>.+)";

            public const string SaveCommandHelpMessage = "_Wrong usage of save command_\n" +
                "Use it such as:\n" +
                "``` /save Home ```\n" +
                "while replying to a location message";

            public const string LocationSavedMessage = "Got it!";

            public static readonly string MaxSaveLocationReachedMessage = "I can't store more than " +
                                                                         CommonConstants.Location.MaxSavedLocations + " locations.\n" +
                                                                         "Try removin a saved location with /del";
        }
    }
}
