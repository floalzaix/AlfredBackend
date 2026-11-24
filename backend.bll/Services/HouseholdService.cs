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
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Le nom du foyer est obligatoire.");

        var normalizedName = name.Trim();
        if (normalizedName.Length > 100)
            throw new BadRequestException("Le nom du foyer ne doit pas dépasser 100 caractères.");

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
}
