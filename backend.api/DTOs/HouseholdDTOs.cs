using System.ComponentModel.DataAnnotations;

namespace Backend.API.DTOs;

public class CreateHouseholdRequestDTO
{
    [Required(ErrorMessage = "Le nom du foyer est obligatoire.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Le nom du foyer doit contenir entre 1 et 100 caractères.")]
    public string Name { get; init; } = string.Empty;
}

public class UpdateHouseholdRequestDTO
{
    [Required(ErrorMessage = "Le nom du foyer est obligatoire.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Le nom du foyer doit contenir entre 1 et 100 caractères.")]
    public string Name { get; init; } = string.Empty;
}

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
