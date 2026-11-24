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
        await GetAdminOrThrowAsync(
            householdId,
            userId,
            "Seul un administrateur du foyer peut créer une pièce."
        );

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

    /**
     * PIE-02: any member of the household (admin or not)
     * can list its rooms.
     */
    public async Task<List<RoomSummary>> GetRoomsAsync(
        Guid userId,
        Guid householdId
    ) {
        await GetMemberOrThrowAsync(householdId, userId);

        return await _roomRepository.GetSummariesByHouseholdAsync(householdId);
    }

    /**
     * PIE-05: rename a room (household admin only).
     */
    public async Task<Room> RenameRoomAsync(
        Guid userId,
        Guid householdId,
        Guid roomId,
        string name
    ) {
        await GetAdminOrThrowAsync(
            householdId,
            userId,
            "Seul un administrateur du foyer peut modifier une pièce."
        );

        Room room = await GetRoomOrThrowAsync(householdId, roomId);
        string trimmedName = name.Trim();

        if (await _roomRepository.ExistsByNameAsync(
            householdId,
            trimmedName,
            excludedRoomId: room.Id
        )) {
            throw new ConflictException(
                "Une pièce porte déjà ce nom dans ce foyer."
            );
        }

        room.Name = trimmedName;
        await _roomRepository.UpdateAsync(room);

        return room;
    }

    /**
     * PIE-05: delete a room (household admin only).
     *
     * The devices of the room must stay in the household
     * without a room: this is enforced by the database
     * (Device.RoomId FK with SetNull, see Database.cs).
     */
    public async Task DeleteRoomAsync(
        Guid userId,
        Guid householdId,
        Guid roomId
    ) {
        await GetAdminOrThrowAsync(
            householdId,
            userId,
            "Seul un administrateur du foyer peut supprimer une pièce."
        );

        Room room = await GetRoomOrThrowAsync(householdId, roomId);

        await _roomRepository.DeleteAsync(room);
    }

    //
    //  Helpers
    //

    /**
     * Member but not admin -> 403 with the given message.
     */
    private async Task<HouseholdMember> GetAdminOrThrowAsync(
        Guid householdId,
        Guid userId,
        string forbiddenMessage
    ) {
        HouseholdMember member = await GetMemberOrThrowAsync(
            householdId,
            userId
        );

        if (member.Role != HouseholdRole.Admin) {
            throw new ForbiddenException(forbiddenMessage);
        }

        return member;
    }

    private async Task<Room> GetRoomOrThrowAsync(Guid householdId, Guid roomId) {
        return await _roomRepository.GetByIdAsync(householdId, roomId)
            ?? throw new NotFoundException("Pièce introuvable.");
    }

    /**
     * Not a member -> 404, so the API does not reveal
     * whether the household exists.
     */
    private async Task<HouseholdMember> GetMemberOrThrowAsync(
        Guid householdId,
        Guid userId
    ) {
        return await _householdRepository.GetMemberAsync(householdId, userId)
            ?? throw new NotFoundException("Foyer introuvable.");
    }
}
