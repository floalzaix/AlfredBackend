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
    public Task<User?> GetUserByEmail(string email);
    public Task CreateUserToken(UserToken userToken);
    public Task<bool> ExistsUserTokenByJti(string jti);
    public Task DeleteUserToken(string jti);
    public Task<int> DeleteExpiredUserTokensAsync();
    public Task<User?> GetUserById(string userId);
}