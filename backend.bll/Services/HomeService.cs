using Backend.BLL.Interfaces;
using Backend.DAL.Interfaces;
using Backend.Models;
using Backend.Models.Exceptions;

namespace Backend.BLL.Services;

public sealed class HomeService(IHomeRepository homeRepository) : IHomeService {
    public Task<Home[]> ListAsync(Guid userId, CancellationToken cancellationToken) {
        return homeRepository.ListForUserAsync(userId, cancellationToken);
    }

    public async Task<HomeMember[]> ListMembersAsync(
        Guid homeId, Guid userId, CancellationToken cancellationToken) {
        var members = await homeRepository.ListMembersForUserAsync(homeId, userId, cancellationToken);

        // An accessible home always includes the requesting user.
        if (members.Length == 0) {
            throw new NotFoundException("Foyer introuvable ou inaccessible.");
        }

        return members;
    }
}
