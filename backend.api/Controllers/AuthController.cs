//
//  Imports
//

using Backend.API.DTOs;
using Backend.BLL.Interfaces;
using Backend.Models;
using System.Security.Claims;

namespace Backend.API.Controllers;

//
//  Class
//

public class AuthController(
    IAuthService authService
) {
    //
    //  Properties
    //

    private readonly IAuthService authService = authService;

    //
    //  Methods
    //

    public async Task Register(RegisterRequestDTO registerRequestDTO) {
        await this.authService.Register(
            registerRequestDTO.Username,
            registerRequestDTO.Email,
            registerRequestDTO.Password
        );
    }
    
    public async Task<IResult> Login(LoginRequestDTO loginRequestDTO) {
        var (_, token) = await this.authService.Login(
            loginRequestDTO.Email,
            loginRequestDTO.Password
        );
        return Results.Ok(new {
            access_token = token,
            token_type = "Bearer"
        });
    }

    public async Task<IResult> Logout(ClaimsPrincipal claimsPrincipal) {
        string? jti = claimsPrincipal.FindFirst("jti")?.Value;
        
        if (jti == null) {
            return Results.Unauthorized();
        }

        await this.authService.Logout(jti);
        return Results.Ok();
    }

    /**
     * A function to get the user's information.
     * 
     * @param claimsPrincipal The claims principal dirived from the JWT token.
     * @return The User.
     */
    public async Task<IResult> Me(ClaimsPrincipal claimsPrincipal) {
        string? userId = claimsPrincipal.FindFirst("sub")?.Value;

        if (userId == null) {
            return Results.Unauthorized();
        }

        // Should not happen (not normal if it does so 500)
        User? user = await this.authService.GetUserById(userId)
            ?? throw new Exception("Utilisateur introuvable.");

        return Results.Ok(new {
            id = user.Id,
            username = user.Username,
            email = user.Email
        });
    }
}