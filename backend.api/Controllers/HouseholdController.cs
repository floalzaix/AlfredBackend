using System.Security.Claims;
using Backend.API.DTOs;
using Backend.BLL.Interfaces;

namespace Backend.API.Controllers;

public class HouseholdController(IHouseholdService householdService)
{
    public async Task<IResult> List(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
            return Results.Unauthorized();

        var households = await householdService.GetHouseholdsForUserAsync(
            userId, cancellationToken);

        var response = households.Select(household =>
            new HouseholdListItemResponseDTO(
                household.Id,
                household.Name,
                household.CreatedAt,
                household.Role.ToString()));

        return Results.Ok(response);
    }

    public async Task<IResult> Create(
        CreateHouseholdRequestDTO request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
            return Results.Unauthorized();

        var household = await householdService.CreateHouseholdAsync(
            request.Name ?? string.Empty, userId, cancellationToken);

        var response = new HouseholdCreatedResponseDTO(
            household.Id,
            household.Name,
            household.InvitationCode,
            household.CreatedAt,
            "Admin");

        return Results.Created($"/api/households/{household.Id}", response);
    }
}
