using Backend.Models;

namespace Backend.DAL.Interfaces;

public interface IHouseholdRepo
{
    Task<IReadOnlyList<HouseholdListItem>> GetHouseholdsForUserAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task<Household> CreateHouseholdWithAdminAsync(
        Household household,
        HouseholdMember adminMembership,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateHouseholdAsync(
        Guid householdId, Guid userId, string name, CancellationToken cancellationToken = default);

    Task<bool> DeleteHouseholdAsync(
        Guid householdId, Guid userId, CancellationToken cancellationToken = default);
}
