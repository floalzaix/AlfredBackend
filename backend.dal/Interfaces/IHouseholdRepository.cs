using Backend.Models;

namespace Backend.DAL.Interfaces;

public enum JoinHouseholdStatus { Joined, AlreadyMember, NotFound }

public record JoinHouseholdResult(JoinHouseholdStatus Status, Household? Household);

public interface IHouseholdRepository {
    // False indicates an invitation-code collision; neither row is persisted.
    Task<bool> CreateAsync(Household household, string creatorId, CancellationToken cancellationToken = default);
    Task<JoinHouseholdResult> JoinAsync(string invitationCode, string userId, CancellationToken cancellationToken = default);
}
