/**
 * This class is used to configure the DAL.
 * 
 * It gives a model for the configuration file that
 * is appsettings.json and when in dev env
 * appsettings.Development.json
 */

//
//  Imports
//

namespace Backend.DAL;

using System.ComponentModel.DataAnnotations;

//
//  Config
//

public class DALConfig {
    [Required(ErrorMessage = "ConnectionString is required in appsettings.json")]
    public string? ConnectionString { get; set; }
}