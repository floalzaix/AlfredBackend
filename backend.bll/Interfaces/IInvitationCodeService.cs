namespace Backend.BLL.Interfaces;

public interface IInvitationCodeService {
    Task<string> GetOrGenerateAsync(Guid householdId, Guid userId);
    Task<Guid> JoinAsync(string invitationCode, Guid userId);
}
