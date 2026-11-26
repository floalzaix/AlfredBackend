/**
 * Typed JWT settings bound from appsettings / user-secrets.
 *
 * Issuer and Audience are shared by token generation (BLL) and
 * token validation (API). Key must come from secrets or env.
 */

//
//  Imports
//

namespace Backend.BLL;

using System.ComponentModel.DataAnnotations;

//
//  Config
//

public class JwtConfig {
    [Required(ErrorMessage = "Issuer is required in JwtConfig")]
    public string? Issuer { get; set; }

    [Required(ErrorMessage = "Audience is required in JwtConfig")]
    public string? Audience { get; set; }

    // At least 32 random bytes, Base64-encoded (e.g. genkey.ps1).
    // Production key must be distinct from the local user-secrets key.
    [Required(ErrorMessage = "Key is required in JwtConfig (user-secrets or env)")]
    [MinLength(44, ErrorMessage = "JwtConfig.Key must be at least 32 bytes Base64 (44 chars)")]
    public string? Key { get; set; }

    [Required(ErrorMessage = "ExpirationTime is required in JwtConfig")]
    public int ExpirationTime { get; set; }
}
