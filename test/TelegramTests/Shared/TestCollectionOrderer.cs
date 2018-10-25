using Framework;

namespace TelegramTests.Shared
{
    public class TestCollectionOrderer : TestCollectionOrdererBase
    {
        private static readonly string[] Collections =
        {
            "channel updates",
            "start command",
        };

        public TestCollectionOrderer()
            : base(Collections) { }
    }
}
