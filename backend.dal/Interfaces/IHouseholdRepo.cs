using Backend.Models;

namespace Backend.DAL.Interfaces;

public interface IHouseholdRepo
{
    Task<Household> CreateHouseholdWithAdminAsync(
        Household household,
        HouseholdMember adminMembership,
        CancellationToken cancellationToken = default);
}
