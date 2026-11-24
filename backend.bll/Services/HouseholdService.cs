using Backend.BLL.Interfaces;
using Backend.DAL.Interfaces;
using Backend.Models;
using Backend.Models.Exceptions;

namespace Backend.BLL.Services;

public class HouseholdService(IHouseholdRepo householdRepository) : IHouseholdService
{
    public Task<IReadOnlyList<HouseholdListItem>> GetHouseholdsForUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return householdRepository.GetHouseholdsForUserAsync(userId, cancellationToken);
    }

    public async Task<Household> CreateHouseholdAsync(
        string name, Guid creatorUserId, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();
        var now = DateTime.UtcNow;
        var household = new Household
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            InvitationCode = null,
            CreatedAt = now
        };

        var adminMembership = new HouseholdMember
        {
            HouseholdId = household.Id,
            UserId = creatorUserId,
            Role = HouseholdRole.Admin,
            JoinedAt = now
        };

        return await householdRepository.CreateHouseholdWithAdminAsync(
            household, adminMembership, cancellationToken);
    }

    public async Task UpdateHouseholdAsync(
        Guid householdId, Guid userId, string name, CancellationToken cancellationToken = default)
    {
        var updated = await householdRepository.UpdateHouseholdAsync(
            householdId, userId, name.Trim(), cancellationToken);
        if (!updated)
            throw new NotFoundException("Foyer introuvable ou droits insuffisants.");
    }

    public async Task DeleteHouseholdAsync(
        Guid householdId, Guid userId, CancellationToken cancellationToken = default)
    {
        var deleted = await householdRepository.DeleteHouseholdAsync(
            householdId, userId, cancellationToken);
        if (!deleted)
            throw new NotFoundException("Foyer introuvable ou droits insuffisants.");
    }
}
