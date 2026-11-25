using Backend.Models;

namespace Backend.DAL.Interfaces;

public interface IHomeRepository {
    Task<Home[]> ListForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<HomeMember[]> ListMembersForUserAsync(
        Guid homeId, Guid userId, CancellationToken cancellationToken);
}
