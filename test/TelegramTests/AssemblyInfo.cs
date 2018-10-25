using Framework;
using TelegramTests.Shared;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

[assembly: TestCollectionOrderer(
    "TelegramTests.Shared." + nameof(TestCollectionOrderer),
    "TelegramTests"
)]

[assembly: TestCaseOrderer(TestConstants.TestCaseOrderer, TestConstants.AssemblyName)]
