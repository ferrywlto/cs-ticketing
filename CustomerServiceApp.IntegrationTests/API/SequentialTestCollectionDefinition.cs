using Xunit;

namespace CustomerServiceApp.IntegrationTests.API;

/// <summary>
/// Collection definition to ensure integration tests run sequentially to avoid database conflicts
/// </summary>
[CollectionDefinition("Sequential Integration Tests", DisableParallelization = true)]
public class SequentialTestCollectionDefinition
{
    // This class has no code; it just serves as a marker for the collection definition
}
