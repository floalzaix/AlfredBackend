using Backend.Models;

namespace Backend.BLL.Interfaces;

public interface IHouseholdService
{
    Task<IReadOnlyList<HouseholdListItem>> GetHouseholdsForUserAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task<Household> CreateHouseholdAsync(
        string name, Guid creatorUserId, CancellationToken cancellationToken = default);

    Task UpdateHouseholdAsync(
        Guid householdId, Guid userId, string name, CancellationToken cancellationToken = default);

    Task DeleteHouseholdAsync(
        Guid householdId, Guid userId, CancellationToken cancellationToken = default);
}
