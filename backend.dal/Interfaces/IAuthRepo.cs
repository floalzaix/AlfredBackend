/**
 * Interface for the AuthRepo.
 */

//
//  Imports
//

namespace Backend.DAL.Interfaces;

using Backend.Models;

//
//  Interface
//

public interface IAuthRepo {
    public Task CreateUser(User user);
    public Task<User> GetUserByEmail(string email);
}