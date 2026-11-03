/**
 * Service for the authentication of the user.
 * 
 * It is meant to validate the user's credentials and
 * return a token if the credentials are valid.
 * 
 * The auth uses JWT bearer tokens with OAuth2.0 flow.
 * 
 * It also allow to register a new user given a username
 * and the personal information.
 */

//
//  Imports
//

namespace Backend.BLL.Services;

//
//  Class
//


public class AuthService {
    public void Login(
        string username,
        string email,
        string password
    ) {
        
    }
}