/**
 * Route for the authentication of the user.
 * 
 * It is meant to give a token to the user with
 * special privileges "User" or "Admin".
 * 
 * This way with the given token in the header of the requests
 * the user can access routes depending on his privileges.
 * 
 * The auth uses JWT Bearer tokens (local validation, no
 * external auth server).
 */

//
//  Imports
//

using System.Security.Claims;

using Backend.API.DTOs;
using Backend.API.Controllers;

namespace Backend.API.Routes;

//
//  Class
//

public static class AuthRoutes {
    public static void MapAuthRoutes(this IEndpointRouteBuilder mainRouter) {
        var authRouter = mainRouter.MapGroup("/auth");

        //
        //  Routes
        //

        authRouter.MapPost("/register", async(
            AuthController authController,
            RegisterRequestDTO registerRequestDTO
        ) => {
            await authController.Register(registerRequestDTO);
            return Results.Ok();
        });

        authRouter.MapPost("/login", async(
            AuthController authController,
            LoginRequestDTO loginRequestDTO
        ) => {
            return await authController.Login(loginRequestDTO);
        });

        authRouter.MapPost("/logout", async(
            AuthController authController,
            ClaimsPrincipal claimsPrincipal
        ) => {
            return await authController.Logout(claimsPrincipal);
        }).RequireAuthorization("User");
    }
}