using System.Security.Claims;
using Backend.API.Controllers;
using Backend.API.DTOs;

namespace Backend.API.Routes;

public static class InvitationCodeRoutes {
    public static void MapInvitationCodeRoutes(this IEndpointRouteBuilder routes) {
        routes.MapPost("/households/{householdId:guid}/invitation-code",
            (Guid householdId, ClaimsPrincipal user, InvitationCodeController controller) =>
                controller.GenerateAsync(householdId, user))
            .RequireAuthorization("User")
            .WithTags("Households")
            .Produces<InvitationCodeDTO>()
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);
    }
}
