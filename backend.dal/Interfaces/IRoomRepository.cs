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
    Task<bool> ExistsByNameAsync(Guid householdId, string name);
    Task AddAsync(Room room);
    Task<List<RoomSummary>> GetSummariesByHouseholdAsync(Guid householdId);
}
