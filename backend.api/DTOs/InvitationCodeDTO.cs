using System.ComponentModel.DataAnnotations;

namespace Backend.API.DTOs;

public sealed record InvitationCodeDTO(string InvitationCode);

public sealed class JoinHouseholdRequestDTO {
    [Required(ErrorMessage = "Le code d’invitation est requis.")]
    [RegularExpression("^[A-Za-z2-9]{16}$", ErrorMessage = "Saisissez les 16 caractères du code d’invitation.")]
    public string InvitationCode { get; init; } = string.Empty;
}

public sealed record JoinedHouseholdDTO(Guid HouseholdId);
