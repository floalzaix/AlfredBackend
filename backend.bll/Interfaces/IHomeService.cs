using Backend.Models;

namespace Backend.BLL.Interfaces;

public interface IHomeService {
    Task<Home[]> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<HomeMember[]> ListMembersAsync(
        Guid homeId, Guid userId, CancellationToken cancellationToken);
}
