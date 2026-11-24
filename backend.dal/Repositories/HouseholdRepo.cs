using Backend.DAL.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.DAL.Repositories;

public class HouseholdRepo(AppDbContext db) : IHouseholdRepo
{
    public async Task<Household> CreateHouseholdWithAdminAsync(
        Household household,
        HouseholdMember adminMembership,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        db.Households.Add(household);
        db.HouseholdMembers.Add(adminMembership);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return household;
    }
}
