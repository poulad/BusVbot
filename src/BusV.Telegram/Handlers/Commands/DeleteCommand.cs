using BusVbot.Models.Cache;
using BusVbot.Services;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusVbot.Handlers.Commands
{
    public class DeleteCommand : CommandBase
    {
        private readonly ILocationsManager _locationsManager;

        public DeleteCommand(
            ILocationsManager locationsManager
        )
        {
            _locationsManager = locationsManager;
        }

        //protected override DeleteCommandArgs ParseInput(Update update)
        //{
        //    var args = base.ParseInput(update);

        //    if (ushort.TryParse(args.ArgsInput, out ushort n))
        //    {
        //        args.Number = n;
        //    }
        //    else
        //    {
        //        args.Name = args.ArgsInput;
        //    }

        //    return args;
        //}

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
            // ToDo only 4 locations allowed
            //public bool IsValid => !string.IsNullOrWhiteSpace(Name) ^
            //                       (Number != null && Number <= 4);

            if (!ushort.TryParse(args[1], out ushort locationNumber))
            {
                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat.Id,
                    Constants.DeleteCommandHelpMessage,
                    ParseMode.Markdown,
                    replyToMessageId: context.Update.Message.MessageId
                );

                return;
            }

            var uc = (UserChat)context.Update;

            var tuple = await _locationsManager.TryRemoveFrequentLocationAsync(uc, (args[1], locationNumber))
                .ConfigureAwait(false);

            string replyText;
            IReplyMarkup replyMarkup = null;

            if (tuple.Exists && tuple.Removed)
            {
                replyText = Constants.LocationRemovedMessage;
                replyMarkup = new ReplyKeyboardRemove();
            }
            else if (tuple.Exists)
            {
                replyText = Constants.LocationRemovalFailed;
            }
            else
            {
                replyText = Constants.LocationNotExist;
            }

            await context.Bot.Client.SendTextMessageAsync(
                context.Update.Message.Chat.Id, replyText,
                ParseMode.Markdown,
                replyToMessageId: context.Update.Message.MessageId,
                replyMarkup: replyMarkup
            ).ConfigureAwait(false);
        }

        private static class Constants
        {
            public const string DeleteCommandHelpMessage = "_Wrong usage of delete command_\n" +
                                                           "Use it such as:\n" +
                                                           "`/del Home`\n" +
                                                           "`/del 1`\n" +
                                                           "Pass it the name of a location or its number (between 1 to 4)";

            public const string LocationRemovedMessage = "Location removed";

            public const string LocationRemovalFailed = "Unable to remove the location";

            public const string LocationNotExist = "I don't remember that location";
        }
    }
}
