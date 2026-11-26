//
//  Imports
//

using System.ComponentModel.DataAnnotations;

namespace Backend.API.DTOs;

//
//  DTO
//

/**
 * Meant to validate the input or create a new user.
 */
public class RegisterRequestDTO
{
    [Required(ErrorMessage = "Le nom d'utilisateur est requis !")]
    [MinLength(3, ErrorMessage = "Le nom d'utilisateur doit contenir au moins 3 caractères !")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis !")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide !")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis !")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères !")]
    [
        RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, " +
            "une minuscule, un chiffre et un caractère spécial !"
        )
    ]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequestDTO
{
    [Required(ErrorMessage = "L'email est requis !")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide !")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis !")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères !")]
    [
        RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, " +
            "une minuscule, un chiffre et un caractère spécial !"
        )
    ]
    public string Password { get; set; } = string.Empty;
}
