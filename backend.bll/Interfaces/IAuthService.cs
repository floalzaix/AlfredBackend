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

public interface IAuthService {
    Task Register(string username, string email, string password);
    Task<(Guid, string)> Login(string email, string password);
}