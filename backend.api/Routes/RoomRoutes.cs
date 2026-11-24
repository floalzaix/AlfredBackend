/**
 * Routes for the rooms of a household.
 *
 * Rooms are nested under their household:
 *   /households/{householdId}/rooms
 */

//
//  Imports
//

using System.Security.Claims;

using Backend.API.DTOs;
using Backend.API.Controllers;

namespace Backend.API.Routes;

//
//  Class
//

public static class RoomRoutes {
    public static void MapRoomRoutes(this IEndpointRouteBuilder mainRouter) {
        var roomRouter = mainRouter
            .MapGroup("/households/{householdId:guid}/rooms")
            .WithTags("Rooms")
            .RequireAuthorization("User");

        //
        //  Routes
        //

        // PIE-02: list the rooms of the household (any member)
        roomRouter.MapGet("/", async (
            RoomController roomController,
            ClaimsPrincipal claimsPrincipal,
            Guid householdId
        ) => {
            return await roomController.GetRooms(claimsPrincipal, householdId);
        })
        .Produces<List<RoomSummaryResponseDTO>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PIE-01: create a room (household admin only)
        roomRouter.MapPost("/", async (
            RoomController roomController,
            ClaimsPrincipal claimsPrincipal,
            Guid householdId,
            CreateRoomRequestDTO createRoomRequestDTO
        ) => {
            return await roomController.CreateRoom(
                claimsPrincipal,
                householdId,
                createRoomRequestDTO
            );
        })
        .Produces<RoomResponseDTO>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
