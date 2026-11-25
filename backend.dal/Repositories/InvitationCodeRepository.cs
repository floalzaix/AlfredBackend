using Backend.DAL.Interfaces;
using Backend.Models;
using Backend.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Backend.DAL.Repositories;

public class InvitationCodeRepository(AppDbContext db) : IInvitationCodeRepository {
    public async Task<Guid?> AddMemberByCodeAsync(string code, Guid userId) {
        await using var transaction = await db.Database.BeginTransactionAsync();
        // Keep the invitation active until the membership insert commits.
        var ids = await db.Database.SqlQuery<Guid>($"""
            SELECT "Id" FROM "Households" WHERE "InvitationCode" = {code} FOR SHARE
            """).ToListAsync();
        if (ids.Count == 0) return null;

        var householdId = ids.Single();
        var inserted = await db.Database.ExecuteSqlAsync($"""
            INSERT INTO "HouseholdMembers" ("HouseholdId", "UserId", "Role", "JoinedAt")
            VALUES ({householdId}, {userId}, {nameof(HouseholdRole.Member)}, {DateTime.UtcNow})
            ON CONFLICT ("HouseholdId", "UserId") DO NOTHING
            """);
        if (inserted == 0)
            throw new ConflictException("Vous êtes déjà membre de ce foyer.");

        await transaction.CommitAsync();
        return householdId;
    }

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
