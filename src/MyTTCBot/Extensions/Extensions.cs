using System.Text.RegularExpressions;
using MyTTCBot.Models.Cache;

namespace MyTTCBot.Extensions
{
    internal static class Extensions
    {
        public static BusDirection? ParseBusDirectionOrNull(this string value)
        {
            if (value is null)
                return null;

            BusDirection? dir;

            switch (value.ToUpper())
            {
                case "NORTH":
                case "N":
                    dir = BusDirection.North;
                    break;
                case "EAST":
                case "E":
                    dir = BusDirection.East;
                    break;
                case "SOUTH":
                case "S":
                    dir = BusDirection.South;
                    break;
                case "WEST":
                case "W":
                    dir = BusDirection.West;
                    break;
                default:
                    dir = null;
                    break;
            }

            return dir;
        }

        public static bool IsValidBusTagRegex(this string value) // todo Remove
        {
            return Regex.IsMatch(value, @"^\d{1,3}[a-z]?$", RegexOptions.IgnoreCase);
        }
    }
}
