/**
 * Example of a route file.
 *
 * A route exposes an HTTP endpoint
 * (ex: http://localhost:<PORT>/example/hello_world) so the app
 * can receive and send data over HTTP/HTTPS. The frontend calls
 * these endpoints.
 *
 * REST naming (required for real features):
 *   REST (Representational State Transfer) means the URL names a
 *   resource (a noun), and the HTTP method names the action.
 *   Prefer plural resource paths, for example:
 *     GET    /devices          → list devices
 *     GET    /devices/{id}     → get one device
 *     POST   /devices          → create a device
 *     PUT    /devices/{id}     → replace a device
 *     PATCH  /devices/{id}     → partially update a device
 *     DELETE /devices/{id}     → delete a device
 *   Avoid verb-based paths like /getDevices or /createDevice.
 *   Nested resources are fine when ownership is clear, e.g.
 *   /homes/{homeId}/rooms.
 *
 * This Example route keeps a demo path (/hello_world). New feature
 * routes should follow the REST style above.
 */

//
//  Imports
//

using Backend.API.Controllers;
using Backend.API.DTOs;

namespace Backend.API.Routes;

//
//  Class
//

public static class ExampleRoutes {

    /**
     * Gives the definitions of the routes to the class which
     * implement IEndpointRouteBuilder.
     * 
     * It is meant to be used in Program.cs using the syntaxe:
     *  app.MapExampleRoutes()
     * 
     * It links the routes to main app giving access to it.
     * 
     * @param None thanks to 'this' -> Check what it is
     * @return Nothing
     */
    public static void MapExampleRoutes(this IEndpointRouteBuilder mainRouter) {
        var exampleRouter = mainRouter.MapGroup("/example");

        //
        //  Routes
        //
        
        /**
         * Demo route (not a REST resource name).
         * Real features should use REST resource paths instead.
         */
        exampleRouter.MapGet("/hello_world", (
            ExampleController controller,
            [AsParameters] GetHelloWorldDTO hw
        ) => {
            return controller.GetHelloWorld(hw);
        });
    }
}
