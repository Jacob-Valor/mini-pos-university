using Xunit;

namespace mini_pos.Tests.Integration;

[CollectionDefinition("Integration", DisableParallelization = true)]
public sealed class IntegrationCollection : ICollectionFixture<MariaDbFixture>
{
}
