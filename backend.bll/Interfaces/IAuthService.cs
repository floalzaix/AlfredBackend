/**
 * Interface for the AuthService
 */

//
//  Imports
//

namespace Backend.BLL.Interfaces;

using Backend.Models;

//
//  Interface
//

public interface IAuthService
{
    Task<string> Register(string username, string email, string password);
    Task<(Guid, string)> Login(string email, string password);
    Task<bool> IsTokenActiveAsync(string jti);
    Task<int> CleanupExpiredTokensAsync();
    Task Logout(string jti);
    Task<User?> GetUserById(string userId);
}
