/**
 * Authorization handlers for protected routes.
 *
 * Checks that the JWT jti is still present in the
 * database so revoked tokens are rejected.
 */

//
//  Imports
//

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Backend.BLL.Interfaces;

namespace Backend.API;

//
//  Requirement
//

public class ActiveJtiRequirement : IAuthorizationRequirement { }

//
//  Handler
//

public class ActiveJtiHandler(
    IAuthService authService
) : AuthorizationHandler<ActiveJtiRequirement> {
    //
    //  Properties
    //

    private readonly IAuthService authService = authService;

    //
    //  Methods
    //

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveJtiRequirement requirement
    ) {
        var jti = context.User.FindFirst(
            JwtRegisteredClaimNames.Jti
        )?.Value
            ?? context.User.FindFirst("jti")?.Value;

        if (jti is null) {
            return;
        }

        if (await this.authService.IsTokenActiveAsync(jti)) {
            context.Succeed(requirement);
        }
    }
}
