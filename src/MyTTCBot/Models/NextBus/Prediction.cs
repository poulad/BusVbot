namespace MyTTCBot.Models.NextBus
{
    public class Prediction
    {
        public bool? IsDeparture { get; set; }

        public int? Minutes { get; set; }

        public int? Seconds { get; set; }

        public string TripTag { get; set; }

        public string Vehicle { get; set; }

        public bool? AffectedByLayover { get; set; }

        public string Block { get; set; }

        public string Branch { get; set; }

        public string DirTag { get; set; }

        public long? EpochTime { get; set; }
    }
}
