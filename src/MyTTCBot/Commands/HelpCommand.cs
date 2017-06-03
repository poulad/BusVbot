using System;
using System.Threading.Tasks;
using NetTelegram.Bot.Framework;
using NetTelegram.Bot.Framework.Abstractions;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public class HelpCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }
    }

    public class HelpCommand : CommandBase<HelpCommandArgs>
    {
        private const string CommandName = "help";

        public HelpCommand()
            : base(CommandName)
        {

        }

        public override Task<UpdateHandlingResult> HandleCommand(Update update, HelpCommandArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
