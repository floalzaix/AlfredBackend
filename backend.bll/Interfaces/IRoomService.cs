/**
 * Interface for the RoomService.
 */

//
//  Imports
//

namespace Backend.BLL.Interfaces;

using Backend.Models;

//
//  Interface
//

public interface IRoomService {
    Task<Room> CreateRoomAsync(Guid userId, Guid householdId, string name);
    Task<List<RoomSummary>> GetRoomsAsync(Guid userId, Guid householdId);
    Task<Room> RenameRoomAsync(
        Guid userId,
        Guid householdId,
        Guid roomId,
        string name
    );
    Task DeleteRoomAsync(Guid userId, Guid householdId, Guid roomId);
}
