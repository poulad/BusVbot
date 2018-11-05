using Newtonsoft.Json;

namespace Framework.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Returns a JSON escaped value e.g. returns "new\\nline" for "new\nline".
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Stringify(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            string json = JsonConvert.SerializeObject(new { _ = value });
            return json.Substring(6, json.Length - 8);
        }
    }
}
