using Backend.DAL.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Backend.DAL.Repositories;

public class InvitationCodeRepository(AppDbContext db) : IInvitationCodeRepository {
    public Task<HouseholdInvitationState?> GetStateAsync(Guid householdId, Guid userId) =>
        (from member in db.HouseholdMembers
         join household in db.Households on member.HouseholdId equals household.Id
         where member.HouseholdId == householdId && member.UserId == userId
         select new HouseholdInvitationState(member.Role, household.InvitationCode))
        .SingleOrDefaultAsync();

    public async Task<bool> TrySetCodeAsync(Guid householdId, Guid userId, string code) {
        try {
            var updated = await db.Households
                .Where(h => h.Id == householdId && h.InvitationCode == null &&
                    db.HouseholdMembers.Any(m => m.HouseholdId == h.Id &&
                        m.UserId == userId && m.Role == HouseholdRole.Admin))
                .ExecuteUpdateAsync(update => update.SetProperty(h => h.InvitationCode, code));
            return updated == 1;
        } catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation &&
            ex.ConstraintName == "IX_Households_InvitationCode") {
            return false;
        }
    }
}
