/**
 * Handles the database interactions for the
 * household and household member models.
 */

//
//  Imports
//

namespace Backend.DAL.Repositories;

using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.DAL.Interfaces;

//
//  Class
//

public class HouseholdRepository(
    AppDbContext _db
) : IHouseholdRepository {
    //
    //  Methods
    //

    public async Task<HouseholdMember?> GetMemberAsync(
        Guid householdId,
        Guid userId
    ) {
        return await _db.HouseholdMembers.SingleOrDefaultAsync(
            m => m.HouseholdId == householdId && m.UserId == userId
        );
    }
}
