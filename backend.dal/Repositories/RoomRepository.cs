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
     *
     * excludedRoomId lets a room be renamed with a different
     * casing of its own name ("salon" -> "Salon").
     */
    public async Task<bool> ExistsByNameAsync(
        Guid householdId,
        string name,
        Guid? excludedRoomId = null
    ) {
        string lowerName = name.ToLower();

        return await _db.Rooms.AnyAsync(
            r => r.HouseholdId == householdId
                && r.Name.ToLower() == lowerName
                && r.Id != excludedRoomId
        );
    }

    /**
     * Scoped to the household so a room id from another
     * household is never returned.
     */
    public async Task<Room?> GetByIdAsync(Guid householdId, Guid roomId) {
        return await _db.Rooms.SingleOrDefaultAsync(
            r => r.Id == roomId && r.HouseholdId == householdId
        );
    }

    public async Task AddAsync(Room room) {
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room) {
        _db.Rooms.Update(room);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Room room) {
        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();
    }

    public async Task<List<RoomSummary>> GetSummariesByHouseholdAsync(
        Guid householdId
    ) {
        return await _db.Rooms
            .Where(r => r.HouseholdId == householdId)
            .OrderBy(r => r.Name)
            .Select(r => new RoomSummary {
                Id = r.Id,
                HouseholdId = r.HouseholdId,
                Name = r.Name,
                // TODO (APP stories): count the devices of the room
                // once the Device model exists, e.g.
                // _db.Devices.Count(d => d.RoomId == r.Id)
                DeviceCount = 0
            })
            .ToListAsync();
    }
}
