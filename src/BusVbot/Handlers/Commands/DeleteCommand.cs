using System.Threading.Tasks;
using BusVbot.Models.Cache;
using BusVbot.Services;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusVbot.Handlers.Commands
{
    public class DeleteCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }

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

            if (ushort.TryParse(args.ArgsInput, out ushort n))
            {
                args.Number = n;
            }
            else
            {
                args.Name = args.ArgsInput;
            }

            return args;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, DeleteCommandArgs args)
        {
            var uc = (UserChat)update;

            #region Validations

            if (!args.IsValid)
            {
                await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id, Constants.DeleteCommandHelpMessage,
                    ParseMode.Markdown,
                    replyToMessageId: update.Message.MessageId);

                return UpdateHandlingResult.Handled;
            }

            #endregion

            ushort n = args.Number ?? default(ushort);
            var tuple = await _locationsManager.TryRemoveFrequentLocationAsync(uc, (args.Name, n));

            string replyText;
            ReplyMarkup replyMarkup = null;

            if (tuple.Exists && tuple.Removed)
            {
                replyText = Constants.LocationRemovedMessage;
                replyMarkup = new ReplyKeyboardRemove { RemoveKeyboard = true };
            }
            else if (tuple.Exists)
            {
                replyText = Constants.LocationRemovalFailed;
            }
            else
            {
                replyText = Constants.LocationDoesntExist;
            }

            await Bot.Client.SendTextMessageAsync(update.Message.Chat.Id, replyText,
                ParseMode.Markdown,
                replyToMessageId: update.Message.MessageId,
                replyMarkup: replyMarkup);

            return UpdateHandlingResult.Handled;
        }

        private static class Constants
        {
            public const string CommandName = "del";

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
