namespace Wit.Ai.Client.Types
{
    public class Context
    {
        public string ReferenceTime { get; set; }

        public string Timezone { get; set; }

        public string Locale { get; set; }

        public Coordinates Coords { get; set; }
    }
}
