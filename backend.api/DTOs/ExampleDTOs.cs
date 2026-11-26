/**
 * This class is meant to validate the input from the example
 * hello world route.
 * 
 * Check how annotations works to understand.
 * 
 * Also there are many annotations so check the ones that
 * correspond to your case. For instance, there is a special
 * annotation for email [EmailAddress(ErrorMessage = "Invalid email !")],
 * for the parameters coming from a GET quey then you need to use the
 * [from_query], and they can be pilled up.
 */

//
//  Imports
//

using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

/**
 * 
 */
namespace Backend.API.DTOs;

//
//  Class
//

public sealed class GetHelloWorldDTO
{
    [FromQuery]
    [Required(ErrorMessage = "Le nom est requis !")]
    [MinLength(1, ErrorMessage = "Le nom ne doit pas être vide !")]
    public string Name { get; init; } = string.Empty;
}
