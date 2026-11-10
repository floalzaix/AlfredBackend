/**
 * Handles all the database interactions for the
 * user model.
 */

//
//  Imports
//

namespace Backend.DAL.Repositories;

using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.DAL;
using Backend.DAL.Interfaces;

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

    public async Task CreateUser(User user) {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task<User> GetUserByEmail(string email) {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        return user is null ? throw new Exception("User not found") : user;
    }
}