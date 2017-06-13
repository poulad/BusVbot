namespace MyTTCBot.Bot
{
    public static class CommonConstants
    {
        public static class Location
        {
            public const string FrequentLocationPrefix = "🚩: ";

            public const string OsmAndLocationRegex = @"geo:([+|-]?\d+(?:.\d+)?),([+|-]?\d+(?:.\d+)?)";

            public const short MaxSavedLocations = 4;
        }

        public static class Direction
        {
            public const string DirectionCallbackQueryPrefix = "↕: ";
        }

        public static class BusRoute
        {
            public const string ValidTtcBusTagRegex = @"^\d{1,3}[a-z]?$";
        }
    }
}
