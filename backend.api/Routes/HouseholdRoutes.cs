using Backend.API.Controllers;
using Backend.API.DTOs;

namespace Backend.API.Routes;

public static class HouseholdRoutes
{
    public static void MapHouseholdRoutes(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/households", async (
            HouseholdController controller,
            System.Security.Claims.ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
                await controller.List(user, cancellationToken))
            .RequireAuthorization("User")
            .WithName("ListHouseholds")
            .Produces<IReadOnlyList<HouseholdListItemResponseDTO>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        routes.MapPost("/households", async (
            HouseholdController controller,
            CreateHouseholdRequestDTO request,
            System.Security.Claims.ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
                await controller.Create(request, user, cancellationToken))
            .RequireAuthorization("User")
            .WithName("CreateHousehold")
            .Produces<HouseholdCreatedResponseDTO>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
