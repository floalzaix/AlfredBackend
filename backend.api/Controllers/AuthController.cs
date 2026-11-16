//
//  Imports
//

using Backend.API.DTOs;
using Backend.BLL.Interfaces;
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
        var (userId, token) = await this.authService.Login(
            loginRequestDTO.Email,
            loginRequestDTO.Password
        );
        return Results.Created("/auth/users/" + userId, new {
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
}