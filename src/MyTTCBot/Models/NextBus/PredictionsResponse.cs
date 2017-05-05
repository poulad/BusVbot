using System.Collections.Generic;

namespace MyTTCBot.Models.NextBus
{
    public class PredictionsResponse
    {
        public Predictions Predictions { get; set; }
    }

    public class Predictions
    {
        public int? RouteTag { get; set; }

        public string RouteTitle { get; set; }

        public Direction Direction { get; set; }

        public string StopTitle { get; set; }

        public string StopTag { get; set; }
    }

    public class Direction
    {
        public string Title { get; set; }

        public IEnumerable<Prediction> Prediction { get; set; }
    }
}
