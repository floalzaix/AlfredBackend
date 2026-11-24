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

    [Required(ErrorMessage = "Key is required in JwtConfig (user-secrets or env)")]
    public string? Key { get; set; }

    [Required(ErrorMessage = "ExpirationTime is required in JwtConfig")]
    public int ExpirationTime { get; set; }
}
