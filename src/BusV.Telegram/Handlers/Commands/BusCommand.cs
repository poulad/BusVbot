using System.Linq;
using System.Threading.Tasks;
using BusV.Data;
using BusV.Telegram.Services;
using Telegram.Bot.Framework.Abstractions;

namespace BusV.Telegram.Handlers.Commands
{
    public class BusCommand : CommandBase
    {
        private readonly IAgencyRepo _agencyRepo;

        public BusCommand(
            IAgencyRepo agencyRepo
        )
        {
            _agencyRepo = agencyRepo;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
            var userProfile = context.GetUserProfile();

            bool hasValidFormat;
            if (args.Length <= 2)
            {
                hasValidFormat = false;
            }
            else
            {
                string cmdArgs = args[0].Substring(args[0].IndexOf(' ') + 1);

                // ToDo include other agencies as well
                const string routeRegex = @"^(?<route>\d{1,3})(?:\s*(?<branch>[A-Z]))?$";
            }

            var agency = await _agencyRepo.GetByIdAsync(userProfile.AgencyDbRef.Id.ToString())
                .ConfigureAwait(false);


//            string argsValue = string.Join(' ', args.Skip(1));
//            var cmdArgs = _predictionsManager.TryParseToRouteDirection(argsValue);
//
//            var userchat = context.Update.ToUserchat();
//
//            if (_predictionsManager.ValidateRouteFormat(cmdArgs.Route))
//            {
//                await _predictionsManager
//                    .CacheRouteDirectionAsync(userchat, cmdArgs.Route, cmdArgs.Direction)
//                    .ConfigureAwait(false);
//
//                await _predictionsManager.TryReplyWithPredictionsAsync(
//                    context.Bot,
//                    userchat,
//                    context.Update.Message.MessageId
//                ).ConfigureAwait(false);
//            }
//            else
//            {
//                string sampleRoutes = await _predictionsManager.GetSampleRouteTextAsync(userchat)
//                    .ConfigureAwait(false);
//
//                await context.Bot.Client.SendTextMessageAsync(
//                    context.Update.Message.Chat.Id,
//                    Constants.InvalidArgumentsMessage + sampleRoutes,
//                    ParseMode.Markdown,
//                    replyToMessageId: context.Update.Message.MessageId
//                ).ConfigureAwait(false);
//            }
        }

        public static class Constants
        {
            public const string InvalidArgumentsMessage = "_Invalid input_\n" +
                                                          "Use bus command in one of these forms:\n";
        }
    }
}
