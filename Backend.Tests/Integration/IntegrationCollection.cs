/**
 * One shared API host for all integration tests.
 *
 * Parallel WebApplicationFactory + SQLite is not safe, so
 * this collection disables parallelization.
 */

//
//  Imports
//

namespace Backend.Tests.Integration;

//
//  Class
//

[CollectionDefinition("Integration", DisableParallelization = true)]
public class IntegrationCollection
    : ICollectionFixture<AlfredWebApplicationFactory>
{
}
