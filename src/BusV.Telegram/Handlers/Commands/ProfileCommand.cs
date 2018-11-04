using System;
using System.Threading.Tasks;
using BusV.Data;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusV.Telegram.Handlers.Commands
{
    /// <summary>
    /// Handles "/profile" command
    /// </summary>
    public class ProfileCommand : CommandBase
    {
        private readonly IAgencyRepo _agencyRepo;
        private readonly ILogger _logger;

        public ProfileCommand(
            IAgencyRepo agencyRepo,
            ILogger<ProfileCommand> logger
        )
        {
            _agencyRepo = agencyRepo;
            _logger = logger;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args)
        {
            var cancellationToken = context.GetCancellationTokenOrDefault();

            if (args.Any() && args[0].Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogTrace(@"Removing user profile upon a ""/profile remove"" command.");

                await context.Bot.Client.SendTextMessageAsync(
                    context.Update.Message.Chat,
                    $"{context.Update.Message.From.FirstName}, You are about to remove your profile. " +
                    "Sadly, I won't remember much of our conversations after the removal.😟\n\n" +
                    "*Are you sure you want to remove your profile?* Reply to this message with the text `forget me`.",
                    ParseMode.Markdown,
                    replyMarkup: new ForceReplyMarkup(),
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                _logger.LogTrace(@"Showing the user his profile upon the ""/profile"" command.");
                var profile = context.GetUserProfile();

                var agency = await _agencyRepo.GetByTagAsync(profile.DefaultAgencyTag, cancellationToken)
                    .ConfigureAwait(false);

                if (agency != null)
                {
                    await context.Bot.Client.SendTextMessageAsync(
                        context.Update.Message.Chat,
                        $"Hey {context.Update.Message.From.FirstName}\n" +
                        $"Your default transit agency is set to *{agency.Title}* " +
                        $"in {agency.Region}, {agency.Country}.\n\n" +
                        "💡 *Pro Tip*: You can remove your profile from my memory by sending " +
                        "the `/profile remove` message.",
                        ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogError(
                        "User has the agency {0} on his profile but the agency is not found in the database.",
                        profile.DefaultAgencyTag
                    );

                    // ToDo clear the profile
                }
            }
        }
    }
}
