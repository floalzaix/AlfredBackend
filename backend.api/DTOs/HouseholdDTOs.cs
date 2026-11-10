namespace Backend.API.DTOs;

public sealed class CreateHouseholdRequest {
    public string? Name { get; init; }
    public string? UserId { get; init; }
}

public sealed class JoinHouseholdRequest {
    public string? Code { get; init; }
    public string? UserId { get; init; }
}
