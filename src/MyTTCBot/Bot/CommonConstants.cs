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

        public static class FlagEmojis
        {
            public const string Canada = "🇨🇦";

            public const string UnitedStates = "🇺🇸";
        }

        public static class CallbackQueries
        {
            public const string CountryPrefix = "c:";

            public const string RegionPrefix = "r:";

            public const string AgencyPrefix = "a:";

            public const string BackToCountries = "_c";

            public const string BackToRegions = "_r_" + CountryPrefix;
        }
    }
}
