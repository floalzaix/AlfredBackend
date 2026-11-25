using System.Security.Claims;
using Backend.API.DTOs;
using Backend.BLL.Interfaces;

namespace Backend.API.Controllers;

public class InvitationCodeController(IInvitationCodeService service) {
    public async Task<IResult> JoinAsync(JoinHouseholdRequestDTO request, ClaimsPrincipal user) {
        if (!Guid.TryParse(user.FindFirst("sub")?.Value, out var userId))
            return Results.Unauthorized();

        var householdId = await service.JoinAsync(request.InvitationCode, userId);
        return Results.Ok(new JoinedHouseholdDTO(householdId));
    }

    public async Task<IResult> GenerateAsync(Guid householdId, ClaimsPrincipal user) {
        if (!Guid.TryParse(user.FindFirst("sub")?.Value, out var userId))
            return Results.Unauthorized();

        var code = await service.GetOrGenerateAsync(householdId, userId);
        return Results.Ok(new InvitationCodeDTO(code));
    }
}
