/**
 * Main script of the project backend.
 *
 * It uses the SwaggerUI as documentation to display the routes.
 * 
 * It implements the Dependency Injection pattern. Using
 * the services.Add<ProjectName>() method. Thanks to the
 * Microsoft.Extension dependency.
 */

//
//  Imports
//

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// Perso

using Backend.API;
using Backend.API.Routes;
using Backend.BLL;

//
//  Builder 
//

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddValidation();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste your JWT here (without the Bearer prefix).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        });
});
builder.Services.AddAPI();

//
//  Auth
//

// Getting the JwtConfig from the configuration both appsettings.json and user-secrets.
var jwtConfig = builder.Configuration
    .GetRequiredSection("JwtConfig")
    .Get<JwtConfig>()
    ?? throw new InvalidOperationException("JwtConfig section is invalid.");

// Adding the authentication service to the builder.
// AddJwtBearer is used to validate the JWT token.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep JWT claim names as issued (role, jti, name, ...).
        // Default remapping turns "role" into a long URI, which
        // breaks RequireRole when RoleClaimType is "role".
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(jwtConfig.Key!)
            ),

            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtConfig.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = "name",
            RoleClaimType = "role"
        };

    });

//
//  CORS
//

var origins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

//
//  Application
//

var app = builder.Build();

//
//  Exception handler
//

app.UseExceptionHandler();

//
//  Path base
//

// Must run before routing, otherwise the endpoints are matched
// again after UseAuthorization and every protected route fails
// with "a middleware was not found that supports authorization".
app.UsePathBase("/api");
app.UseRouting();

//
//  CORS usage
//

app.UseCors("Frontend");

//
//  Auth usage
//

app.UseAuthentication();
app.UseAuthorization();

//
//  OpenAPI gen
//

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

//
//  Routes
//

app.MapExampleRoutes();
app.MapAuthRoutes();

// Adding the SwaggerUI route
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
