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
    }
}
