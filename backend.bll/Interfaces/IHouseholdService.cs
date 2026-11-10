using Backend.DAL.Interfaces;
using Backend.Models;

namespace Backend.BLL.Interfaces;

public interface IHouseholdService
{
    Task<Household> CreateAsync(string name, string userId, CancellationToken cancellationToken = default);
    Task<JoinHouseholdResult> JoinAsync(string code, string userId, CancellationToken cancellationToken = default);
}
