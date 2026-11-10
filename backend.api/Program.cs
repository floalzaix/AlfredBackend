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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("User", policy =>
        policy.RequireRole("User", "Admin"));
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));
});

//
//  Application
//

var app = builder.Build();

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
