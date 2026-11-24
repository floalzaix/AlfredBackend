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
}
