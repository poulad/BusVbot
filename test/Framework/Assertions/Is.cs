using Moq;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Framework
{
    public static class Is
    {
        public static T SameJson<T>(string json)
            => It.Is<T>(actual => JsonEqual(json, actual));

        private static bool JsonEqual<T>(string expected, T actual)
        {
            var expectedToken = JToken.Parse(expected);
            var actualToken = JToken.FromObject(actual);

            return JToken.DeepEquals(expectedToken, actualToken);
        }
    }
}
