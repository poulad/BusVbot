using Telegram.Bot.Types;

namespace BusV.Telegram.Models.Cache
{
    public class CacheUserContext2
    {
        public UserChatContext UserChatContext { get; set; }

        public Location Location { get; set; }

        public BusCommandArgs BusCommandArgs { get; set; }

        public bool ProfileSetupInstructionsSent { get; set; }

        public int AgencyId { get; set; }

        public string AgencyTag { get; set; }
    }

    public class BusCommandArgs
    {
        public string RouteTag { get; set; }
        public string DirectionName { get; set; }
    }
}
