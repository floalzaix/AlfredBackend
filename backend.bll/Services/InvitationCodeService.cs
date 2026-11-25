using System.Security.Cryptography;
using Backend.BLL.Interfaces;
using Backend.DAL.Interfaces;
using Backend.Models;
using Backend.Models.Exceptions;

namespace Backend.BLL.Services;

public class InvitationCodeService(IInvitationCodeRepository repository) : IInvitationCodeService {
    public async Task<Guid> JoinAsync(string invitationCode, Guid userId) =>
        await repository.AddMemberByCodeAsync(invitationCode.ToUpperInvariant(), userId)
            ?? throw new NotFoundException("Code d’invitation introuvable.");

    public async Task<string> GetOrGenerateAsync(Guid householdId, Guid userId) {
        while (true) {
            var state = await repository.GetStateAsync(householdId, userId)
                ?? throw new NotFoundException("Foyer introuvable.");
            if (state.Role != HouseholdRole.Admin)
                throw new ForbiddenException("Seul un administrateur du foyer peut générer un code d’invitation.");
            if (state.Code is not null) return state.Code;

            var code = RandomNumberGenerator.GetString("ABCDEFGHJKLMNPQRSTUVWXYZ23456789", 16);
            if (await repository.TrySetCodeAsync(householdId, userId, code)) return code;
            // Another request set the code, membership changed, or the unique index rejected a collision.
        }
    }
}
