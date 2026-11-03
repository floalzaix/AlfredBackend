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
public class RegisterRequestDTO {
    [Required(ErrorMessage = "The username is required !")]
    [MinLength(3, ErrorMessage = "The username must be at least 3 characters long !")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "The email is required !")]
    [EmailAddress(ErrorMessage = "The email is not valid !")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "The password is required !")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long !")]
    [
        RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "The password must contain at least one uppercase letter, " +
            "one lowercase letter, one number and one special character !"
        )
    ]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequestDTO {
    [Required(ErrorMessage = "The email is required !")]
    [EmailAddress(ErrorMessage = "The email is not valid !")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "The password is required !")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long !")]
    [
        RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "The password must contain at least one uppercase letter, " +
            "one lowercase letter, one number and one special character !"
        )
    ]
    public string Password { get; set; } = string.Empty;
}