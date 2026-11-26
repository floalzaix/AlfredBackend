/**
 * Unit tests for AuthService.
 *
 * The repository and password hasher are mocked with Moq so
 * these tests only check the business rules of the service
 * (conflict, bad credentials, JWT claims, token revoke, …).
 */

//
//  Imports
//

namespace Backend.Tests.Unit.Services;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Backend.BLL;
using Backend.BLL.Services;
using Backend.DAL.Interfaces;
using Backend.Models;
using Backend.Models.Exceptions;

//
//  Class
//

public class AuthServiceTests {
    //
    //  Properties
    //

    // 32 bytes Base64 (>= 44 chars) — matches JwtConfig rules.
    private const string TestJwtKey =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    private readonly Mock<IAuthRepo> authRepo = new();
    private readonly Mock<IPasswordHasher<User>> passwordHasher = new();
    private readonly AuthService authService;

    //
    //  Constructor
    //

    public AuthServiceTests() {
        var jwtOptions = Options.Create(new JwtConfig {
            Issuer = "alfred_backend_test",
            Audience = "alfred_backend_test",
            Key = TestJwtKey,
            ExpirationTime = 60
        });

        this.authService = new AuthService(
            this.passwordHasher.Object,
            this.authRepo.Object,
            jwtOptions
        );
    }

    //
    //  Methods — Register
    //

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldThrowConflict() {
        this.authRepo
            .Setup(r => r.GetUserByEmail("taken@example.com"))
            .ReturnsAsync(new User {
                Id = Guid.NewGuid(),
                Username = "existing",
                Email = "taken@example.com",
                PasswordHash = "hash",
                Role = "User"
            });

        ConflictException ex = await Assert.ThrowsAsync<ConflictException>(
            () => this.authService.Register(
                "new-user",
                "taken@example.com",
                "Password1!"
            )
        );

        Assert.Equal("Email déjà utilisé.", ex.Message);
        Assert.Equal(409, ex.StatusCode);
        this.authRepo.Verify(
            r => r.CreateUser(It.IsAny<User>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Register_WhenEmailIsFree_ShouldPersistHashedUserAndReturnJwt() {
        User? createdUser = null;
        UserToken? createdToken = null;

        this.authRepo
            .Setup(r => r.GetUserByEmail("free@example.com"))
            .ReturnsAsync((User?)null);
        this.passwordHasher
            .Setup(h => h.HashPassword(It.IsAny<User>(), "Password1!"))
            .Returns("hashed-password");
        this.authRepo
            .Setup(r => r.CreateUser(It.IsAny<User>()))
            .Callback<User>(user => createdUser = user)
            .Returns(Task.CompletedTask);
        this.authRepo
            .Setup(r => r.CreateUserToken(It.IsAny<UserToken>()))
            .Callback<UserToken>(token => createdToken = token)
            .Returns(Task.CompletedTask);

        string jwt = await this.authService.Register(
            "alice",
            "free@example.com",
            "Password1!"
        );

        Assert.NotNull(createdUser);
        Assert.Equal("alice", createdUser!.Username);
        Assert.Equal("free@example.com", createdUser.Email);
        Assert.Equal("hashed-password", createdUser.PasswordHash);
        Assert.Equal("User", createdUser.Role);
        Assert.NotEqual("Password1!", createdUser.PasswordHash);

        Assert.NotNull(createdToken);
        Assert.Equal(createdUser.Id, createdToken!.UserId);
        Assert.False(string.IsNullOrWhiteSpace(createdToken.Jti));
        Assert.True(createdToken.ExpiresAt > DateTime.UtcNow);

        JwtSecurityToken parsed = new JwtSecurityTokenHandler()
            .ReadJwtToken(jwt);

        Assert.Equal("alfred_backend_test", parsed.Issuer);
        Assert.Contains("alfred_backend_test", parsed.Audiences);
        Assert.Equal(
            createdUser.Id.ToString(),
            parsed.Claims.First(
                c => c.Type == JwtRegisteredClaimNames.Sub
            ).Value
        );
        Assert.Equal(
            "alice",
            parsed.Claims.First(c => c.Type == "name").Value
        );
        Assert.Equal(
            "User",
            parsed.Claims.First(c => c.Type == "role").Value
        );
        Assert.Equal(
            createdToken.Jti,
            parsed.Claims.First(
                c => c.Type == JwtRegisteredClaimNames.Jti
            ).Value
        );
    }

    //
    //  Methods — Login
    //

    [Fact]
    public async Task Login_WhenUserMissing_ShouldThrowUnauthorized() {
        this.authRepo
            .Setup(r => r.GetUserByEmail("unknown@example.com"))
            .ReturnsAsync((User?)null);

        UnauthorizedException ex = await Assert.ThrowsAsync<UnauthorizedException>(
            () => this.authService.Login(
                "unknown@example.com",
                "Password1!"
            )
        );

        // Same message as a wrong password on purpose
        // (do not leak whether the email exists).
        Assert.Equal("Mot de passe invalide.", ex.Message);
        Assert.Equal(401, ex.StatusCode);
        this.authRepo.Verify(
            r => r.CreateUserToken(It.IsAny<UserToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Login_WhenPasswordIsWrong_ShouldThrowUnauthorized() {
        var user = new User {
            Id = Guid.NewGuid(),
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = "hashed",
            Role = "User"
        };

        this.authRepo
            .Setup(r => r.GetUserByEmail(user.Email))
            .ReturnsAsync(user);
        this.passwordHasher
            .Setup(h => h.VerifyHashedPassword(
                user,
                user.PasswordHash,
                "WrongPass1!"
            ))
            .Returns(PasswordVerificationResult.Failed);

        UnauthorizedException ex = await Assert.ThrowsAsync<UnauthorizedException>(
            () => this.authService.Login(user.Email, "WrongPass1!")
        );

        Assert.Equal("Mot de passe invalide.", ex.Message);
        this.authRepo.Verify(
            r => r.CreateUserToken(It.IsAny<UserToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnUserIdAndPersistToken() {
        var user = new User {
            Id = Guid.NewGuid(),
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = "hashed",
            Role = "User"
        };
        UserToken? createdToken = null;

        this.authRepo
            .Setup(r => r.GetUserByEmail(user.Email))
            .ReturnsAsync(user);
        this.passwordHasher
            .Setup(h => h.VerifyHashedPassword(
                user,
                user.PasswordHash,
                "Password1!"
            ))
            .Returns(PasswordVerificationResult.Success);
        this.authRepo
            .Setup(r => r.CreateUserToken(It.IsAny<UserToken>()))
            .Callback<UserToken>(token => createdToken = token)
            .Returns(Task.CompletedTask);

        (Guid userId, string jwt) = await this.authService.Login(
            user.Email,
            "Password1!"
        );

        Assert.Equal(user.Id, userId);
        Assert.False(string.IsNullOrWhiteSpace(jwt));
        Assert.NotNull(createdToken);
        Assert.Equal(user.Id, createdToken!.UserId);

        JwtSecurityToken parsed = new JwtSecurityTokenHandler()
            .ReadJwtToken(jwt);

        Assert.Equal(
            user.Id.ToString(),
            parsed.Claims.First(
                c => c.Type == JwtRegisteredClaimNames.Sub
            ).Value
        );
        Assert.Equal(
            createdToken.Jti,
            parsed.Claims.First(
                c => c.Type == JwtRegisteredClaimNames.Jti
            ).Value
        );
    }

    //
    //  Methods — Token helpers
    //

    [Fact]
    public async Task IsTokenActiveAsync_ShouldAskTheRepository() {
        this.authRepo
            .Setup(r => r.ExistsUserTokenByJti("active-jti"))
            .ReturnsAsync(true);

        bool isActive = await this.authService.IsTokenActiveAsync(
            "active-jti"
        );

        Assert.True(isActive);
        this.authRepo.Verify(
            r => r.ExistsUserTokenByJti("active-jti"),
            Times.Once
        );
    }

    [Fact]
    public async Task Logout_ShouldDeleteTokenByJti() {
        this.authRepo
            .Setup(r => r.DeleteUserToken("to-revoke"))
            .Returns(Task.CompletedTask);

        await this.authService.Logout("to-revoke");

        this.authRepo.Verify(
            r => r.DeleteUserToken("to-revoke"),
            Times.Once
        );
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_ShouldReturnDeletedCount() {
        this.authRepo
            .Setup(r => r.DeleteExpiredUserTokensAsync())
            .ReturnsAsync(3);

        int deleted = await this.authService.CleanupExpiredTokensAsync();

        Assert.Equal(3, deleted);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnRepositoryResult() {
        var user = new User {
            Id = Guid.NewGuid(),
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = "hashed",
            Role = "User"
        };

        this.authRepo
            .Setup(r => r.GetUserById(user.Id.ToString()))
            .ReturnsAsync(user);

        User? result = await this.authService.GetUserById(
            user.Id.ToString()
        );

        Assert.Same(user, result);
    }
}
