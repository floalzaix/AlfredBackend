/**
 * Depency injection file.
 * 
 * It is declared in Program.cs as a Scope.
 * 
 * Here every interface that you add thanks to the method
 * AddScoped will be injected with the requested class.
 * 
 * It meants, that for instance if you have a controller and that a certain  
 * route or else needs it (meaning that the controller is in its class        
 * constructor args), if you ask for a certain IController in the constructot  
 * (interface of the required controller) then when the controller will be     
 * asked in a route or else then a Controller respecting the IController       
 * contract will be created and injected in the route or else. Therefore, no  
 * need to pass the controller arg to the route or else when creating it.
 * 
 * The goals of dependency injection are:
 *   - To provide objects with their dependencies instead of having
 *     them create those dependencies themselves.
 *   - To reduce coupling to concrete implementations, making
 *     dependencies easier to replace and code easier to test.
 *
 * Using a dependency injection also allows us:
 *   - To centralise dependency registration and manage instance
 *     lifetimes, such as singleton, scoped, and transient.
 *   - To automatically construct dependency graphs, avoiding
 *     repetitive manual object creation and wiring.
 */

using Microsoft.Extensions.DependencyInjection;

namespace Backend.API;

public static class DependencyInjection {
    public static IServiceCollection AddAPI(this IServiceCollection services) {
        // services.AddScoped<Interface, Class(implementating the interface)>();
        // ex : services.AddScoped<IController, Controller>();
        // 
        // Here the Controller class should implement the IController interface, and if
        // a route or else needs the controller, then it will be automatically given to
        // it no need to pass it.
        //
        // The purpose here is to have the cleanest objects possible -> Every time that
        // you need a controller or any other objects then it has the fewest args to be
        // passed to it, just the logic ones, like the n_iteration or stuff like that.   
        
        return services;
    }
}