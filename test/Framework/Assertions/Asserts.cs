using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Framework
{
    public static class Asserts
    {
        public static void JsonEqual<T>(T expected, T actual)
        {
            bool equals = JToken.DeepEquals(
                JToken.FromObject(expected),
                JToken.FromObject(actual)
            );

            if (!equals)
            {
                // throws an exception with a consistent message from xUnit
                Assert.Equal(expected, actual);
            }
        }

        public static void JsonEqual(string expected, string actual)
        {
            var expectedJ = JToken.Parse(expected);
            var actualJ = JToken.Parse(actual);
            bool equals = JToken.DeepEquals(expectedJ, actualJ);

            if (!equals)
            {
                // throws an exception with a consistent message from xUnit
                Assert.Equal(expectedJ.ToString(), actualJ.ToString());
            }
        }

        public static void IsJson(string value)
        {
            bool isJson;
            try
            {
                JToken.Parse(value);
                isJson = true;
            }
            catch (JsonReaderException)
            {
                isJson = false;
            }

            Assert.True(isJson, "Invalid JSON value");
        }
    }
}
