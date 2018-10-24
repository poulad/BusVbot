namespace BusV.Telegram.Models.Cache
{
    public class CacheContext
    {
        public UserProfileSetupCache ProfileSetup { get; set; }
    }

    public class UserProfileSetupCache
    {
        public bool IsInstructionsSent { get; set; }
    }
}


//using BusV.Telegram.Models;
//using Telegram.Bot.Types;
//
//namespace BusV.Ops.Cache
//{
//    public class CacheContext
//    {
//        public UserChatContext UserChatContext { get; set; }
//
//        public Location Location { get; set; }
//
//        public BusCommandArgs BusCommandArgs { get; set; }
//
//        public bool ProfileSetupInstructionsSent { get; set; }
//
//        public int AgencyId { get; set; }
//
//        public string AgencyTag { get; set; }
//    }
//
//    public class BusCommandArgs
//    {
//        public string RouteTag { get; set; }
//        public string DirectionName { get; set; }
//    }
//
//    // ToDo move to its own file
//
//    public enum TtcBusDirection
//    {
//        North,
//        East,
//        South,
//        West
//    }
//}
