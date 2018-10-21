using System;
using System.Linq;

namespace BusV.Telegram.Configurations
{
    public class AgencyTimeZonesAccessor
    {
        public TimeZoneInfo this[string agencyTag]
        {
            get
            {
                string[] timezoneIds = AgencyTimezones
                    .Where(pair => pair.Agencies.Contains(agencyTag, StringComparer.OrdinalIgnoreCase))
                    .Select(pair => pair.Timezones)
                    .FirstOrDefault();

                if (timezoneIds == null)
                {
                    return null;
                }

                var allTimezones = TimeZoneInfo.GetSystemTimeZones();

                TimeZoneInfo timezone = allTimezones
                    .SingleOrDefault(tz => timezoneIds.Contains(tz.Id, StringComparer.OrdinalIgnoreCase));

                return timezone;
            }
        }

        public AgencyTimezonePair[] AgencyTimezones { get; set; }
    }

    public class AgencyTimezonePair
    {
        public string[] Agencies { get; set; }

        public string[] Timezones { get; set; }
    }
}
