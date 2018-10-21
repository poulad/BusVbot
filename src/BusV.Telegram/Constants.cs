namespace BusV.Telegram
{
    public static class Constants
    {
        public static class Location
        {
            public const string FrequentLocationPrefix = "🚩: ";

            public const string OsmAndLocationRegex = @"geo:([+|-]?\d+(?:.\d+)?),([+|-]?\d+(?:.\d+)?)";

            public const short MaxSavedLocations = 4;
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
            public static class UserProfileSetup
            {
                public const string UserProfileSetupPrefix = "ups/";

                public const string CountryPrefix = UserProfileSetupPrefix + "c:"; // ups/c:{country}

                public const string RegionPrefix = UserProfileSetupPrefix + "r:"; // ups/r:{name}

                public const string AgencyPrefix = UserProfileSetupPrefix + "a:"; // ups/a:{id}

                public const string
                    BackToRegionsForCountryPrefix = UserProfileSetupPrefix + "r/c:"; // ups/r/c:{country}

                public const string BackToCountries = UserProfileSetupPrefix + "c"; // ups/c
            }

            public static class BusCommand
            {
                public const string BusCommandPrefix = "bus/";

                public const string BusDirectionPrefix = BusCommandPrefix + "d:"; // bus/d:{direction}
            }

            public static class Prediction
            {
                public const string PredictionPrefix = "prd/";

                public const string PredictionRegex = "^prd/.*\0.*\0.*$"; // prd/{agency}\0{route}\0{direction}

                public const char PredictionValuesDelimiter = '\0';
            }
        }
    }
}