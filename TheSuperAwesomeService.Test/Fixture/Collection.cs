using Xunit;

namespace TheSuperAwesomeService.Test.Fixture
{
    [CollectionDefinition("BaseCollection")]
    public class Collection : ICollectionFixture<TestContex> { }
}
