/**
 * Handles the database interactions for the room model.
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

public class RoomRepository(
    AppDbContext _db
) : IRoomRepository {
    //
    //  Methods
    //

    /**
     * Case-insensitive check so "Salon" and "salon" are
     * considered the same room in a household.
     */
    public async Task<bool> ExistsByNameAsync(Guid householdId, string name) {
        string lowerName = name.ToLower();

        return await _db.Rooms.AnyAsync(
            r => r.HouseholdId == householdId && r.Name.ToLower() == lowerName
        );
    }

    public async Task AddAsync(Room room) {
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
    }
}
