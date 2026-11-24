/**
 * Interface for the RoomRepository.
 */

//
//  Imports
//

namespace Backend.DAL.Interfaces;

using Backend.Models;

//
//  Interface
//

public interface IRoomRepository {
    Task<bool> ExistsByNameAsync(
        Guid householdId,
        string name,
        Guid? excludedRoomId = null
    );
    Task<Room?> GetByIdAsync(Guid householdId, Guid roomId);
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(Room room);
    Task<List<RoomSummary>> GetSummariesByHouseholdAsync(Guid householdId);
}
