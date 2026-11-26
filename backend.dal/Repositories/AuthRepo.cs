/**
 * Handles all the database interactions for the
 * user model.
 */

//
//  Imports
//

namespace Backend.DAL.Repositories;

using Backend.DAL;
using Backend.DAL.Interfaces;
using Backend.Models;

using Microsoft.EntityFrameworkCore;

//
//  Class
//

public class AuthRepo(AppDbContext db) : IAuthRepo
{
    //
    //  Properties
    //

    private readonly AppDbContext db = db;

    //
    //  Methods
    //

    public async Task CreateUser(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task CreateUserToken(UserToken userToken)
    {
        db.UserTokens.Add(userToken);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsUserTokenByJti(string jti)
    {
        DateTime now = DateTime.UtcNow;

        return await db.UserTokens.AnyAsync(
            ut => ut.Jti == jti && ut.ExpiresAt > now
        );
    }

    public async Task DeleteUserToken(string jti)
    {
        UserToken? userToken = await db.UserTokens.SingleOrDefaultAsync(
            ut => ut.Jti == jti
        );

        if (userToken is not null)
        {
            db.UserTokens.Remove(userToken);
            await db.SaveChangesAsync();
        }
    }

    public async Task<int> DeleteExpiredUserTokensAsync()
    {
        DateTime now = DateTime.UtcNow;

        return await db.UserTokens
            .Where(ut => ut.ExpiresAt <= now)
            .ExecuteDeleteAsync();
    }

    public async Task<User?> GetUserById(string userId)
    {
        return await db.Users.FindAsync(new Guid(userId));
    }
}
