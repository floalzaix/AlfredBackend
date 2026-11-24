/**
 * Interface for the HouseholdRepository.
 *
 * Only exposes what the room feature needs for now
 * (membership lookup). The FOY / MEM stories will extend it.
 */

//
//  Imports
//

namespace Backend.DAL.Interfaces;

using Backend.Models;

//
//  Interface
//

public interface IHouseholdRepository {
    Task<HouseholdMember?> GetMemberAsync(Guid householdId, Guid userId);
}
