/**
 * Unit tests for ExampleService.
 *
 * Checks what the demo service is supposed to return
 * (name + a real Guid), not only that the call succeeds.
 */

//
//  Imports
//

namespace Backend.Tests.Unit.Services;

using Backend.BLL.Services;
using Backend.Models;

//
//  Class
//

public class ExampleServiceTests
{
    //
    //  Methods
    //

    [Fact]
    public void GetExempleUser_ShouldReturnUserWithRequestedName()
    {
        var service = new ExampleService();

        ExempleUser result = service.GetExempleUser("Alice");

        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public void GetExempleUser_ShouldAssignNonEmptyId()
    {
        var service = new ExampleService();

        ExempleUser result = service.GetExempleUser("Bob");

        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public void GetExempleUser_ShouldAssignDistinctIdsAcrossCalls()
    {
        var service = new ExampleService();

        ExempleUser first = service.GetExempleUser("Same");
        ExempleUser second = service.GetExempleUser("Same");

        Assert.NotEqual(first.Id, second.Id);
    }
}
