using Backend.Models;

namespace Backend.BLL.Interfaces;

public interface IHouseholdService
{
    Task<IReadOnlyList<HouseholdListItem>> GetHouseholdsForUserAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task<Household> CreateHouseholdAsync(
        string name, Guid creatorUserId, CancellationToken cancellationToken = default);
}
