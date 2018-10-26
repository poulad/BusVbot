using Framework;

namespace TelegramTests.Shared
{
    public class TestCollectionOrderer : TestCollectionOrdererBase
    {
        private static readonly string[] Collections =
        {
            "channel updates",
            "start command",
            "user profile setup",
        };

        public TestCollectionOrderer()
            : base(Collections) { }
    }
}
