namespace BusV.Data
{
    public static class Constants
    {
        public static class Collections
        {
            public static class Agencies
            {
                public const string Name = "agencies";

                public static class Indexes
                {
                    public const string AgencyName = "agency_id";
                }
            }

            public static class Registrations
            {
                public const string Name = "registrations";

                public static class Indexes
                {
                    public const string BotUsername = "bot_username";
                }
            }
        }
    }
}
