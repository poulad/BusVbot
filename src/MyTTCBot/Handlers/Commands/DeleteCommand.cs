using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyTTCBot.Models.Cache;
using NetTelegram.Bot.Framework;
using NetTelegram.Bot.Framework.Abstractions;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;
using MyTTCBot.Services;

namespace MyTTCBot.Handlers.Commands
{
    public class DeleteCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string Name { get; set; }

        public ushort? Number { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Name) ^
            (Number != null && Number <= 4);
    }

    public class DeleteCommand : CommandBase<DeleteCommandArgs>
    {
        private readonly ILocationsManager _locationsManager;

        public DeleteCommand(ILocationsManager locationsManager)
            : base(Constants.CommandName)
        {
            _locationsManager = locationsManager;
        }

        protected override DeleteCommandArgs ParseInput(Update update)
        {
            var args = base.ParseInput(update);

            var matches = Regex.Match(update.Message.Text, Constants.DeleteCommandRegex);
            if (matches.Success)
            {
                var val = matches.Groups["id"].Value.Trim();
                if (ushort.TryParse(val, out ushort n))
                {
                    args.Number = n;
                }
                else
                {
                    args.Name = val;
                }
            }

            return args;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DeleteCommandArgs args)
        {
            var uc = (UserChat)update;

            #region Validations

            if (!args.IsValid)
            {
                await Bot.MakeRequest(new SendMessage(update.Message.Chat.Id, Constants.DeleteCommandHelpMessage)
                {
                    ReplyToMessageId = update.Message.MessageId,
                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                });
                return UpdateHandlingResult.Handled;
            }

            #endregion

            ushort n = args.Number ?? default(ushort);
            var tuple = await _locationsManager.TryRemoveFrequentLocation(uc, (args.Name, n));

            SendMessage request = new SendMessage(update.Message.Chat.Id, string.Empty)
            {
                ReplyToMessageId = update.Message.MessageId,
            };

            if (tuple.Exists && tuple.Removed)
            {
                request.Text = Constants.LocationRemovedMessage;
                request.ReplyMarkup = new ReplyKeyboardRemove { RemoveKeyboard = true };
            }
            else if (tuple.Exists)
            {
                request.Text = Constants.LocationRemovalFailed;
            }
            else
            {
                request.Text = Constants.LocationDoesntExist;
            }

            await Bot.MakeRequest(request);

            return UpdateHandlingResult.Handled;
        }

        private static class Constants
        {
            public const string CommandName = "del";

            public const string DeleteCommandRegex = "^/" + CommandName + @"\s+(?<id>.+)";

            public const string DeleteCommandHelpMessage = "_Wrong usage of delete command_\n" +
                                                           "Use it such as:\n" +
                                                           "`/del Home`\n" +
                                                           "`/del 1`\n" +
                                                           "Pass it the name of a location or its number (between 1 to 4)";

            public const string LocationRemovedMessage = "Location removed";

            public const string LocationRemovalFailed = "Unable to remove the location";

            public const string LocationDoesntExist = "I don't remember that location";
        }
    }
}
