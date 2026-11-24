/**
 * Service for the rooms of a household (PIE stories).
 *
 * Access rules rely on the user's role inside the household
 * (HouseholdMember.Role), not on the global JWT role:
 *   - not a member of the household -> 404 (do not reveal
 *     that the household exists)
 *   - member but not admin          -> 403
 */

//
//  Imports
//

namespace Backend.BLL.Services;

using Backend.Models;
using Backend.Models.Exceptions;
using Backend.BLL.Interfaces;
using Backend.DAL.Interfaces;

//
//  Class
//

public class RoomService(
    IRoomRepository _roomRepository,
    IHouseholdRepository _householdRepository
) : IRoomService {
    //
    //  Methods
    //

    public async Task<Room> CreateRoomAsync(
        Guid userId,
        Guid householdId,
        string name
    ) {
        HouseholdMember? member = await _householdRepository.GetMemberAsync(
            householdId,
            userId
        );

        if (member is null) {
            throw new NotFoundException("Foyer introuvable.");
        }

        if (member.Role != HouseholdRole.Admin) {
            throw new ForbiddenException(
                "Seul un administrateur du foyer peut créer une pièce."
            );
        }

        string trimmedName = name.Trim();

        if (await _roomRepository.ExistsByNameAsync(householdId, trimmedName)) {
            throw new ConflictException(
                "Une pièce porte déjà ce nom dans ce foyer."
            );
        }

        var room = new Room {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            Name = trimmedName
        };

        await _roomRepository.AddAsync(room);

        return room;
    }
}
