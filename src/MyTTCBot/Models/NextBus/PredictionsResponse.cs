using System.Collections.Generic;

namespace MyTTCBot.Models.NextBus
{
    public class PredictionsResponse
    {
        public PredictionsResponsePredictions Predictions { get; set; }

        public class PredictionsResponsePredictions
        {
            public string RouteTag { get; set; }

            public string RouteTitle { get; set; }

            public string StopTitle { get; set; }

            public string StopTag { get; set; }

            public IEnumerable<PredictionsResponsePredictionsDirection> Direction { get; set; }

            public class PredictionsResponsePredictionsDirection
            {
                public string Title { get; set; }

                public IEnumerable<PredictionsResponsePredictionsDirectionPrediction> Prediction { get; set; }

                public class PredictionsResponsePredictionsDirectionPrediction
                {
                    public bool IsDeparture { get; set; }

                    public int Minutes { get; set; }

                    public int Seconds { get; set; }

                    public string TripTag { get; set; }

                    public string Vehicle { get; set; }

                    public bool? AffectedByLayover { get; set; }

                    public string Block { get; set; }

                    public string Branch { get; set; }

                    public string DirTag { get; set; }

                    public long? EpochTime { get; set; }
                }
            }
        }
    }
}
