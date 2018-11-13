using Framework;

namespace TelegramTests.Shared
{
    public class TestCollectionOrderer : TestCollectionOrdererBase
    {
        private static readonly string[] Collections =
        {
            "channel updates",

            "/start command",
            "/cancel command",

            "user profile setup",
            "user profile setup menu",
            "/profile command",
            "profile removal",

            "/bus command",
            "bus predictions",
        };

        public TestCollectionOrderer()
            : base(Collections) { }
    }
}
