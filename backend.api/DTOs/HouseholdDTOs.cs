namespace Backend.API.DTOs;

public record CreateHouseholdRequestDTO(string? Name);

public record HouseholdCreatedResponseDTO(
    Guid Id,
    string Name,
    string? InvitationCode,
    DateTime CreatedAt,
    string Role);

public record HouseholdListItemResponseDTO(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    string Role);
