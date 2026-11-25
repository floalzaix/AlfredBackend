using System.Security.Claims;
using Backend.API.DTOs;
using Backend.BLL.Interfaces;

namespace Backend.API.Controllers;

public sealed class HomeController(IHomeService homeService) {
    public async Task<IResult> ListAsync(
        ClaimsPrincipal principal, CancellationToken cancellationToken) {
        if (!Guid.TryParse(principal.FindFirst("sub")?.Value, out var userId)) {
            return Results.Unauthorized();
        }

        var homes = await homeService.ListAsync(userId, cancellationToken);
        return Results.Ok(homes.Select(home => new HomeDto(home.Id, home.Name)).ToArray());
    }

    public async Task<IResult> ListMembersAsync(
        Guid homeId, ClaimsPrincipal principal, CancellationToken cancellationToken) {
        if (!Guid.TryParse(principal.FindFirst("sub")?.Value, out var userId)) {
            return Results.Unauthorized();
        }

        var members = await homeService.ListMembersAsync(homeId, userId, cancellationToken);
        return Results.Ok(members.Select(member =>
            new HomeMemberDto(member.Username, member.Role.ToString())).ToArray());
    }
}
