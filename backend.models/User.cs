/**
 * User model.
 * 
 * It is used to store the user information in the database.
 * Linked to the authentication through the PasswordHasher.
 */

//
//  Imports
//

using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

//
//  Class
//

public class User {
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}