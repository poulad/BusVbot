using System.Text.RegularExpressions;
using BusVbot.Models;

// ReSharper disable once CheckNamespace
namespace BusVbot.Services.Agency
{
    public class TtcDataParser : AgencyDataParserBase
    {
        public TtcDataParser(BusVbotDbContext dbContext)
            :  base(dbContext, Constants.Tag, Constants.SampleRoutesText)
        {
            
        }

        public override (bool Success, string RouteTag) TryParseToRouteTag(string routeText)
        {
            bool isValid;
            string routeTag = null;

            if (string.IsNullOrWhiteSpace(routeText))
            {
                return (false, null);
            }

            var match = Regex.Match(routeText, Constants.RouteRegex, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                routeTag = match.Groups[0].Value;
            }

            isValid = match.Success;

            return (isValid, routeTag);
        }

        public override (bool Success, string DirectionName) TryParseToDirectionName(string routeTag, string directionText)
        {
            if (string.IsNullOrWhiteSpace(directionText))
            {
                return (false, null);
            }

            bool isValid = true;
            string direction;

            switch (directionText.ToUpper())
            {
                case "N":
                case "NORTH":
                case "NORTHBOUND":
                case "NORTH BOUND":
                    direction = "north";
                    break;
                case "S":
                case "SOUTH":
                case "SOUTHBOUND":
                case "SOUTH BOUND":
                    direction = "south";
                    break;
                case "E":
                case "EAST":
                case "EASTBOUND":
                case "EAST BOUND":
                    direction = "east";
                    break;
                case "W":
                case "WEST":
                case "WESTBOUND":
                case "WEST BOUND":
                    direction = "west";
                    break;
                default:
                    isValid = false;
                    direction = null;
                    break;
            }

            return (isValid, direction);
        }

        private static class Constants
        {
            public const string Tag = "ttc";

            public const string SampleRoutesText = "```\n" +
                                                   "/bus 110 North\n" +
                                                   "/bus 110 n\n" +
                                                   "/bus 110\n" +
                                                   "```";

            public const string RouteRegex = @"^(?<route>\d{1,3})(?:\s*(?<branch>[A-Z]))?$";
        }
    }
}
