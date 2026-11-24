using Backend.Models;

namespace Backend.BLL.Interfaces;

public interface IHouseholdService
{
    Task<Household> CreateHouseholdAsync(
        string name, Guid creatorUserId, CancellationToken cancellationToken = default);
}
