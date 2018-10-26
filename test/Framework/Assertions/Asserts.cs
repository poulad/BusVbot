using System;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace Framework.Assertions
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

        public static Expression<Func<T, bool>> JsonEqual<T>(T expected) =>
            actual => _JsonEqual(expected, actual);

        private static bool _JsonEqual<T>(T expected, T actual)
        {
            try
            {
                JsonEqual(expected, actual);
                return true;
            }
            catch (AssertActualExpectedException)
            {
                return false;
            }
        }
    }
}
