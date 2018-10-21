using BusV.Data.Entities;

namespace BusV.Ops
{
    internal static class Converter
    {
        public static Agency FromNextBusAgency(NextBus.NET.Models.Agency nextbusAgency) =>
            new Agency
            {
                Tag = NonEmptyValue(nextbusAgency.Tag),
                Title = NonEmptyValue(nextbusAgency.Title),
                ShortTitle = NonEmptyValue(nextbusAgency.ShortTitle),
                Region = NonEmptyValue(nextbusAgency.RegionTitle),
            };

        private static string NonEmptyValue(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
