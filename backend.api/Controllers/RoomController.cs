/**
 * Controller for the rooms of a household.
 *
 * Reads the current user id from the JWT ("sub" claim) and
 * delegates the business rules to the RoomService.
 */

//
//  Imports
//

using System.Security.Claims;

using Backend.API.DTOs;
using Backend.BLL.Interfaces;
using Backend.Models;
using Backend.Models.Exceptions;

namespace Backend.API.Controllers;

//
//  Class
//

public class RoomController(
    IRoomService _roomService
) {
    //
    //  Methods
    //

    public async Task<IResult> CreateRoom(
        ClaimsPrincipal claimsPrincipal,
        Guid householdId,
        CreateRoomRequestDTO createRoomRequestDTO
    ) {
        Room room = await _roomService.CreateRoomAsync(
            GetUserId(claimsPrincipal),
            householdId,
            createRoomRequestDTO.Name
        );

        return Results.Created(
            $"/api/households/{householdId}/rooms/{room.Id}",
            ToResponse(room)
        );
    }

    public async Task<IResult> GetRooms(
        ClaimsPrincipal claimsPrincipal,
        Guid householdId
    ) {
        List<RoomSummary> rooms = await _roomService.GetRoomsAsync(
            GetUserId(claimsPrincipal),
            householdId
        );

        return Results.Ok(rooms.Select(r => new RoomSummaryResponseDTO {
            Id = r.Id,
            HouseholdId = r.HouseholdId,
            Name = r.Name,
            DeviceCount = r.DeviceCount
        }));
    }

    public async Task<IResult> RenameRoom(
        ClaimsPrincipal claimsPrincipal,
        Guid householdId,
        Guid roomId,
        UpdateRoomRequestDTO updateRoomRequestDTO
    ) {
        Room room = await _roomService.RenameRoomAsync(
            GetUserId(claimsPrincipal),
            householdId,
            roomId,
            updateRoomRequestDTO.Name
        );

        return Results.Ok(ToResponse(room));
    }

    public async Task<IResult> DeleteRoom(
        ClaimsPrincipal claimsPrincipal,
        Guid householdId,
        Guid roomId
    ) {
        await _roomService.DeleteRoomAsync(
            GetUserId(claimsPrincipal),
            householdId,
            roomId
        );

        return Results.NoContent();
    }

    //
    //  Helpers
    //

    private static Guid GetUserId(ClaimsPrincipal claimsPrincipal) {
        string? sub = claimsPrincipal.FindFirst("sub")?.Value;

        if (!Guid.TryParse(sub, out Guid userId)) {
            throw new UnauthorizedException();
        }

        return userId;
    }

    private static RoomResponseDTO ToResponse(Room room) {
        return new RoomResponseDTO {
            Id = room.Id,
            HouseholdId = room.HouseholdId,
            Name = room.Name
        };
    }
}
