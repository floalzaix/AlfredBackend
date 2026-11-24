using Backend.DAL.Interfaces;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.DAL.Repositories;

public sealed class HomeRepository(AppDbContext db) : IHomeRepository {
    public Task<Home[]> ListForUserAsync(Guid userId, CancellationToken cancellationToken) {
        return (
            from membership in db.HomeMemberships
            join home in db.Homes on membership.HomeId equals home.Id
            where membership.UserId == userId
            orderby home.Name, home.Id
            select home
        ).AsNoTracking().ToArrayAsync(cancellationToken);
    }

    public Task<HomeMember[]> ListMembersForUserAsync(
        Guid homeId, Guid userId, CancellationToken cancellationToken) {
        var homesForUser = db.HomeMemberships
            .Where(membership => membership.UserId == userId)
            .Select(membership => membership.HomeId);

        return (
            from membership in db.HomeMemberships
            join user in db.Users on membership.UserId equals user.Id
            where membership.HomeId == homeId && homesForUser.Contains(membership.HomeId)
            orderby user.Username, user.Id
            select new HomeMember(user.Username, membership.Role)
        ).ToArrayAsync(cancellationToken);
    }
}
