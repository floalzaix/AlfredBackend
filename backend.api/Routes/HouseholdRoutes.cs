using Backend.API.Controllers;
using Backend.API.DTOs;

namespace Backend.API.Routes;

public static class HouseholdRoutes
{
    public static void MapHouseholdRoutes(this IEndpointRouteBuilder routes)
    {
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
