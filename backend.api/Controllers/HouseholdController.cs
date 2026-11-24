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
        if (!TryGetUserId(user, out var userId))
            return Results.Unauthorized();

        var households = await householdService.GetHouseholdsForUserAsync(userId, cancellationToken);
        var response = households.Select(household =>
            new HouseholdListItemResponseDTO(
                household.Id, household.Name, household.CreatedAt, household.Role.ToString()));
        return Results.Ok(response);
    }

    public async Task<IResult> Create(
        CreateHouseholdRequestDTO request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(user, out var userId))
            return Results.Unauthorized();

        var household = await householdService.CreateHouseholdAsync(
            request.Name, userId, cancellationToken);
        var response = new HouseholdCreatedResponseDTO(
            household.Id, household.Name, household.InvitationCode, household.CreatedAt, "Admin");
        return Results.Created($"/api/households/{household.Id}", response);
    }

    public async Task<IResult> Update(
        Guid id,
        UpdateHouseholdRequestDTO request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(user, out var userId))
            return Results.Unauthorized();

        await householdService.UpdateHouseholdAsync(id, userId, request.Name, cancellationToken);
        return Results.NoContent();
    }

    public async Task<IResult> Delete(
        Guid id,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(user, out var userId))
            return Results.Unauthorized();

        await householdService.DeleteHouseholdAsync(id, userId, cancellationToken);
        return Results.NoContent();
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId) =>
        Guid.TryParse(user.FindFirst("sub")?.Value, out userId);
}
