namespace MyTTCBot.Commands
{
    public interface IBotCommand : IMessageHandler
    {
        string Name { get; }
    }
}
