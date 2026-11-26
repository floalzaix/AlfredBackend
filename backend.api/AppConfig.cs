/**
 * Typed API host settings bound from appsettings / env.
 *
 * UseHttpsRedirection should be false on Azure App Service when
 * HTTPS Only terminates TLS at the reverse proxy.
 */

//
//  Imports
//

namespace Backend.API;

//
//  Config
//

public class AppConfig {
    // false on Azure App Service when HTTPS Only terminates TLS at the proxy
    public bool UseHttpsRedirection { get; set; } = true;
}
