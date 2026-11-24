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
}

