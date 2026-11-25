using System.Security.Claims;
using Backend.API.Controllers;
using Backend.API.DTOs;

namespace Backend.API.Routes;

public static class HomeRoutes {
    public static void MapHomeRoutes(this IEndpointRouteBuilder mainRouter) {
        var homeRouter = mainRouter.MapGroup("/homes").RequireAuthorization("User");

        homeRouter.MapGet("", (
            HomeController controller, ClaimsPrincipal principal, CancellationToken cancellationToken
        ) => controller.ListAsync(principal, cancellationToken))
            .Produces<HomeDto[]>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        homeRouter.MapGet("/{homeId:guid}/members", (
            Guid homeId, HomeController controller, ClaimsPrincipal principal, CancellationToken cancellationToken
        ) => controller.ListMembersAsync(homeId, principal, cancellationToken))
            .Produces<HomeMemberDto[]>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
