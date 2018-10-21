namespace BusV.Telegram.Options
{
    /// <summary>
    /// Contains application settings for connecting to a data store
    /// </summary>
    public class DataOptions
    {
        /// <summary>
        /// MongoDB connection string 
        /// </summary>
        public string ConnectionString { get; set; }
    }
}