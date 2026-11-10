using Backend.DAL.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Backend.DAL.Repositories;

public class HouseholdRepository(AppDbContext db) : IHouseholdRepository {
    public async Task<bool> CreateAsync(Household household, string creatorId, CancellationToken cancellationToken = default) {
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember { Household = household, UserId = creatorId });

        try {
            // SaveChanges uses one transaction for the household and its creator.
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, "UX_Households_InvitationCode")) {
            db.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<JoinHouseholdResult> JoinAsync(string invitationCode, string userId, CancellationToken cancellationToken = default) {
        var household = await db.Households.AsNoTracking()
            .SingleOrDefaultAsync(h => h.InvitationCode == invitationCode, cancellationToken);
        if (household is null) return new(JoinHouseholdStatus.NotFound, null);

        var member = new HouseholdMember { HouseholdId = household.Id, UserId = userId };
        db.HouseholdMembers.Add(member);
        try {
            await db.SaveChangesAsync(cancellationToken);
            return new(JoinHouseholdStatus.Joined, household);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception, "PK_HouseholdMembers")) {
            db.Entry(member).State = EntityState.Detached;
            return new(JoinHouseholdStatus.AlreadyMember, household);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception, string constraintName) =>
        exception.InnerException is PostgresException postgres &&
        postgres.SqlState == PostgresErrorCodes.UniqueViolation &&
        postgres.ConstraintName == constraintName;
}
