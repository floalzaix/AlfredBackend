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
    public string ConnectionString { get; set; } = string.Empty;

    [Required(ErrorMessage = "DB Password required in env !")]
    public string DBPassword { get; set; } = string.Empty;

    //
    //  Methods
    //
    
    /**
     * Get the full connection string with the db password in the
     * string by replacing the place holder.
     * 
     * @return The connection string to the db ready to be used.
     */
    public string GetFullConnectionString() {
        return this.ConnectionString.Replace("{PASSWORD_PLACEHOLDER}", this.DBPassword);
    }
}