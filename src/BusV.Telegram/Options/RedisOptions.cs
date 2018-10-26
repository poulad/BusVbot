namespace BusV.Telegram.Options
{
    /// <summary>
    /// Contains application settings for connecting to a Redis instance
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// Redis connection configuration. For more info see https://stackexchange.github.io/StackExchange.Redis/Configuration
        /// </summary>
        public string Configuration { get; set; }
    }
}
