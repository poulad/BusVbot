using BusV.Telegram;
using Moq;
using Telegram.Bot;

namespace TelegramTests.Shared
{
    public class MockBot : BusVbot
    {
        public MockBot(IMock<ITelegramBotClient> mockClient)
            : base("BusVbot", mockClient.Object) { }
    }
}
