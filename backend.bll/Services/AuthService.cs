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

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Backend.Models;
using Backend.BLL.Interfaces;
using Backend.DAL.Interfaces;

//
//  Class
//


public class AuthService(
    IPasswordHasher<User> passwordHasher,
    IAuthRepo userRepository,
    IOptions<JwtConfig> jwtConfig
) : IAuthService {
    //
    //  Properties
    //
    
    private readonly IPasswordHasher<User> passwordHasher = passwordHasher;
    private readonly IAuthRepo userRepository = userRepository;
    private readonly JwtConfig jwtConfig = jwtConfig.Value;

    //
    //  Methods
    //

    public async Task Register(
        string username,
        string email,
        string password
    ) {
        var user = new User {
            Username = username,
            Email = email,
            PasswordHash = string.Empty,
            Role = "User"
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);

        await this.userRepository.CreateUser(user);
    }

    public async Task<(Guid, string)> Login(string email, string password) {
        User user = await this.userRepository.GetUserByEmail(email);

        if (passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password
        ) != PasswordVerificationResult.Success) {
            throw new Exception("Invalid password !");
        }

        return (user.Id, this.generateToken(user));
    }

    private string generateToken(User user) {
        var key = new SymmetricSecurityKey(
            Convert.FromBase64String(jwtConfig.Key!)
        );

        var token = new JwtSecurityToken(
            issuer: jwtConfig.Issuer,
            audience: jwtConfig.Audience,
            claims: [
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim("role", user.Role),
            ],
            expires: DateTime.UtcNow.AddMinutes(
                jwtConfig.ExpirationTime
            ),
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}