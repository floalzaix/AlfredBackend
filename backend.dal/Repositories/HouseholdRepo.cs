using Backend.DAL.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.DAL.Repositories;

public class HouseholdRepo(AppDbContext db) : IHouseholdRepo
{

    public async Task<IReadOnlyList<HouseholdListItem>> GetHouseholdsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await db.HouseholdMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .OrderByDescending(member => member.Household.CreatedAt)
            .Select(member => new HouseholdListItem(
                member.Household.Id,
                member.Household.Name,
                member.Household.CreatedAt,
                member.Role))
            .ToListAsync(cancellationToken);
    }

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
