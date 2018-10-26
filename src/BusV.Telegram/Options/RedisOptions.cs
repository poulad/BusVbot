namespace BusV.Telegram.Options
{
    /// <summary>
    /// Contains application settings for connecting to a Redis instance
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// Redis connection string
        /// </summary>
        public string Endpoint { get; set; }
    }
}
