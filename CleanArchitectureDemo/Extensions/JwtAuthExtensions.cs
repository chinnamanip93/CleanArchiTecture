using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitectureDemo.Extensions
{
    public static class JwtAuthExtensions
    {
        /// <summary>
        /// Adds JWT Bearer authentication and role-based authorization policies.
        /// Reads from appsettings.json → JwtSettings section.
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection("JwtSettings");
            var secretKey = section["SecretKey"]
                            ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
            var issuer = section["Issuer"] ?? "CleanArchitectureDemo";
            var audience = section["Audience"] ?? "CleanArchitectureDemo";

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero   // no grace period on expiry
                    };

                    // Return clean JSON for API clients instead of HTML redirects
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/problem+json";
                            return context.Response.WriteAsync(
                                """{"status":401,"title":"Unauthorized","detail":"A valid Bearer token is required."}""");
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/problem+json";
                            return context.Response.WriteAsync(
                                """{"status":403,"title":"Forbidden","detail":"You do not have permission to perform this action."}""");
                        }
                    };
                });

            // Named authorization policies
            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
                .AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));

            return services;
        }
    }
}
