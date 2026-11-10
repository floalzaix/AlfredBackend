using Backend.API.DTOs;
using Backend.BLL.Interfaces;
using Backend.DAL.Interfaces;
using Backend.Models;

namespace Backend.API.Controllers;

public class HouseholdController(IHouseholdService householdService) {
    public Task<Household> CreateAsync(CreateHouseholdRequest request, CancellationToken cancellationToken) =>
        householdService.CreateAsync(request.Name!.Trim(), request.UserId!.Trim(), cancellationToken);

    public Task<JoinHouseholdResult> JoinAsync(JoinHouseholdRequest request, CancellationToken cancellationToken) =>
        householdService.JoinAsync(request.Code!.Trim(), request.UserId!.Trim(), cancellationToken);
}
