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
        return await (
            from member in db.HouseholdMembers.AsNoTracking()
            join household in db.Households.AsNoTracking()
                on member.HouseholdId equals household.Id
            where member.UserId == userId
            orderby household.CreatedAt descending
            select new HouseholdListItem(
                household.Id,
                household.Name,
                household.CreatedAt,
                member.Role)
        ).ToListAsync(cancellationToken);
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

    public async Task<bool> UpdateHouseholdAsync(
        Guid householdId, Guid userId, string name, CancellationToken cancellationToken = default)
    {
        var household = await (
            from item in db.Households
            join member in db.HouseholdMembers
                on item.Id equals member.HouseholdId
            where item.Id == householdId && member.UserId == userId && member.Role == HouseholdRole.Admin
            select item
        ).SingleOrDefaultAsync(cancellationToken);

        if (household is null)
            return false;

        household.Name = name;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteHouseholdAsync(
        Guid householdId, Guid userId, CancellationToken cancellationToken = default)
    {
        var household = await (
            from item in db.Households
            join member in db.HouseholdMembers
                on item.Id equals member.HouseholdId
            where item.Id == householdId && member.UserId == userId && member.Role == HouseholdRole.Admin
            select item
        ).SingleOrDefaultAsync(cancellationToken);

        if (household is null)
            return false;

        db.Households.Remove(household);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
