using Backend.API.Controllers;
using Backend.API.DTOs;
using Backend.DAL.Interfaces;

namespace Backend.API.Routes;

public static class HouseholdRoutes {
    public static void MapHouseholdRoutes(this IEndpointRouteBuilder router) {
        var households = router.MapGroup("/households");

        households.MapPost("/", async (CreateHouseholdRequest request, HouseholdController controller, CancellationToken cancellationToken) => {
            var errors = Validate(request.Name, 100, "name");
            AddErrors(errors, request.UserId, 128, "userId");
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var household = await controller.CreateAsync(request, cancellationToken);
            return Results.Json(household, statusCode: StatusCodes.Status201Created);
        });

        households.MapPost("/join", async (JoinHouseholdRequest request, HouseholdController controller, CancellationToken cancellationToken) => {
            var errors = Validate(request.Code, 16, "code");
            AddErrors(errors, request.UserId, 128, "userId");
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var result = await controller.JoinAsync(request, cancellationToken);
            return result.Status switch {
                JoinHouseholdStatus.Joined => Results.Ok(result.Household),
                JoinHouseholdStatus.AlreadyMember => Results.Conflict(new { error = "The user is already a member of this household." }),
                JoinHouseholdStatus.NotFound => Results.NotFound(new { error = "No household matches this code." }),
                _ => Results.Problem("An unexpected result occurred.")
            };
        });
    }

    private static Dictionary<string, string[]> Validate(string? value, int maxLength, string field) {
        var errors = new Dictionary<string, string[]>();
        AddErrors(errors, value, maxLength, field);
        return errors;
    }

    private static void AddErrors(Dictionary<string, string[]> errors, string? value, int maxLength, string field) {
        if (string.IsNullOrWhiteSpace(value)) {
            errors[field] = ["This field is required."];
        } else if (value.Trim().Length > maxLength) {
            errors[field] = [$"This field must be at most {maxLength} characters long."];
        }
    }
}
