using Moq;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Framework
{
    public static class Is
    {
        public static T SameJson<T>(string json)
            => It.Is<T>(actual => JsonEqual(json, actual));

        private static bool JsonEqual<T>(string json, T actual)
            => JToken.DeepEquals(
                JToken.Parse(json),
                JToken.FromObject(actual)
            );
    }
}
