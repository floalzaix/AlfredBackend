/**
 * Service for the authentication of the user.
 * 
 * It is meant to validate the user's credentials and
 * return a token if the credentials are valid.
 * 
 * The auth uses JWT Bearer tokens (local validation, no
 * external auth server).
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
using Backend.Models.Exceptions;
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
        User? user = await this.userRepository.GetUserByEmail(email);

        bool isPasswordValid = user is not null
            && passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            ) == PasswordVerificationResult.Success;

        if (!isPasswordValid) {
            throw new UnauthorizedException("Invalid credentials.");
        }

        return (user!.Id, await this.generateToken(user));
    }

    private async Task<string> generateToken(User user) {
        var key = new SymmetricSecurityKey(
            Convert.FromBase64String(jwtConfig.Key!)
        );

        string jti = Guid.NewGuid().ToString();
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(
            jwtConfig.ExpirationTime
        );

        var token = new JwtSecurityToken(
            issuer: jwtConfig.Issuer,
            audience: jwtConfig.Audience,
            claims: [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim("role", user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
            ],
            expires: expiresAt,
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            )
        );

        // Persists the token id to the database to handle the logout process.
        await this.userRepository.CreateUserToken(new UserToken {
            Id = Guid.NewGuid(),
            Jti = jti,
            UserId = user.Id,
            ExpiresAt = expiresAt
        });

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> IsTokenActiveAsync(string jti) {
        return await this.userRepository.ExistsUserTokenByJti(jti);
    }

    public async Task<int> CleanupExpiredTokensAsync() {
        return await this.userRepository.DeleteExpiredUserTokensAsync();
    }

    public async Task Logout(string jti) {
        await this.userRepository.DeleteUserToken(jti);
    }
}