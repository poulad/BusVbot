using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MyTTCBot.Models;
using MyTTCBot.Models.NextBus;
using MyTTCBot.Services;
using NetTelegram.Bot.Framework;
using NetTelegram.Bot.Framework.Abstractions;
using NetTelegramBotApi.Requests;
using NetTelegramBotApi.Types;

namespace MyTTCBot.Commands
{
    public class BusCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string BusTag { get; set; }

        public BusDirection? BusDirection { get; set; }
    }

    public class BusCommand : CommandBase<BusCommandArgs>
    {
        private const string CommandName = "bus";

        private readonly INextBusService _nextBusService;

        private readonly IMemoryCache _cache;

        public BusCommand(INextBusService nextBusService, IMemoryCache cache)
            : base(CommandName)
        {
            _nextBusService = nextBusService;
            _cache = cache;
        }

        protected override BusCommandArgs ParseInput(Update update)
        {
            var tokens = Regex.Split(update.Message.Text, @"\s+");
            if (tokens.Length == 3)
            {
                var args = new BusCommandArgs
                {
                    RawInput = update.Message.Text,
                    BusTag = tokens[1]
                };

                switch (tokens[2].ToUpper()[0])
                {
                    case 'N':
                        args.BusDirection = BusDirection.North;
                        break;
                    case 'E':
                        args.BusDirection = BusDirection.East;
                        break;
                    case 'W':
                        args.BusDirection = BusDirection.West;
                        break;
                    case 'S':
                        args.BusDirection = BusDirection.South;
                        break;
                }
                return args;
            }

            return null;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(Update update, BusCommandArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.BusTag))
            {
                // ToDo: Show error message
                return UpdateHandlingResult.Handled;
            }

            if (args.BusDirection is null)
            {
                // ToDo: Reply error
                return UpdateHandlingResult.Handled;
            }

            var userChat = new UserChat(update.Message.From.Id, update.Message.Chat.Id);
            UserContext context;
            if (!_cache.TryGetValue(userChat, out context))
            {
                await Bot.MakeRequestAsync(new SendMessage(update.Message.Chat.Id, "Send your location")
                {
                    ReplyToMessageId = update.Message.MessageId,
                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                })
                    .ConfigureAwait(false);
                return UpdateHandlingResult.Handled;
            }

            // ToDo: Call NextBus to validate bus stop

            BusStop nearestStop;
            try
            {
                nearestStop = await _nextBusService.FindNearestBusStop(
                        args.BusTag, args.BusDirection.GetValueOrDefault(), context.Location.Longitude, context.Location.Latitude)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // invalid bus stop
                Debug.WriteLine(e);
                await Bot.MakeRequestAsync(new SendMessage(update.Message.Chat.Id, "__Wrong bus stop!__")
                {
                    ReplyToMessageId = update.Message.MessageId,
                    ParseMode = SendMessage.ParseModeEnum.Markdown,
                })
                    .ConfigureAwait(false);
                return UpdateHandlingResult.Handled;
            }


            var predictionsResponse = await _nextBusService.GetPredictions(args.BusTag, nearestStop.Id);
            var locationMessage = await Bot.MakeRequestAsync(
                new SendLocation(update.Message.Chat.Id, (float)nearestStop.Latitude, (float)nearestStop.Longitude)
                {
                    ReplyToMessageId = update.Message.MessageId
                });
            var reply = FormatResponse(locationMessage, predictionsResponse?.Predictions?.Direction?.First().Prediction, predictionsResponse?.Predictions?.Direction?.First().Title);
            await Bot.MakeRequestAsync(reply)
                .ConfigureAwait(false);
            return UpdateHandlingResult.Handled;
        }

        private static RequestBase<Message> FormatResponse(Message messageInReply, IEnumerable<PredictionsResponse.PredictionsResponsePredictions.PredictionsResponsePredictionsDirection.PredictionsResponsePredictionsDirectionPrediction> predictions, string busTitle)
        {
            string replyText;

            if (predictions == null)
            {
                replyText = Constants.PredictionNotFoundMessage;
            }
            else
            {
                var easternTimeZone = TimeZoneInfo.GetSystemTimeZones().Single(x =>
                        string.Equals(x.Id, "America/Toronto", StringComparison.OrdinalIgnoreCase) || // for GNU/Linux
                        string.Equals(x.Id, "Eastern Standard Time", StringComparison.OrdinalIgnoreCase) // For windows
                );

                var predictionsArray = predictions.ToArray();
                var predictionsSchedule = string.Empty;
                foreach (var prediction in predictionsArray)
                {
                    var utcTime = DateTime.UtcNow.AddSeconds(prediction.Seconds);
                    var easternTime = TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, easternTimeZone);
                    var formattedMinutes = prediction.Minutes < 10 ? " " + prediction.Minutes : prediction.Minutes + "";
                    predictionsSchedule += string.Format(Constants.PredictionsScheduleFormat + '\n',
                        easternTime, formattedMinutes, prediction.Minutes < 2 ? "" : "s");
                }

                replyText = string.Format(Constants.PredictionsMessageFormat, busTitle, predictionsSchedule);
            }
            var response = new SendMessage(messageInReply.Chat.Id, replyText)
            {
                ReplyToMessageId = messageInReply.MessageId,
                ParseMode = SendMessage.ParseModeEnum.Markdown,
            };
            return response;
        }

        public static class Constants
        {
            public const string PredictionNotFoundMessage = "__Sorry! Can't find any predictions__";

            public const string PredictionsMessageFormat = "Bus *{0}*:\n\n{1}";

            public const string PredictionsScheduleFormat = "`{0:hh:mm}` *-* `{1}` minute{2}";
        }
    }
}
