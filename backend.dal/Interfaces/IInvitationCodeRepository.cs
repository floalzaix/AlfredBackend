using Backend.Models;

namespace Backend.DAL.Interfaces;

public sealed record HouseholdInvitationState(HouseholdRole Role, string? Code);

public interface IInvitationCodeRepository {
    Task<HouseholdInvitationState?> GetStateAsync(Guid householdId, Guid userId);
    Task<bool> TrySetCodeAsync(Guid householdId, Guid userId, string code);
}
