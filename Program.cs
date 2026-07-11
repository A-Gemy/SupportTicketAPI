using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SupportTicketAPI.Authorization.Handlers;
using SupportTicketAPI.Authorization.Requirements;
using SupportTicketAPI.Constants;
using SupportTicketAPI.DataAccess;
using SupportTicketAPI.DataAccess.Interfaces;
using SupportTicketAPI.Security;
using SupportTicketAPI.Services;
using SupportTicketAPI.Services.Interfaces;
using System.Text;
using System.Threading.RateLimiting;


namespace SupportTicketAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is missing.");
            }

            builder.Services.AddAuthentication(options =>
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

                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero // Optional: reduce default clock skew
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    AuthorizationPolicies.CanViewTicketComments,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.AddRequirements(
                            new TicketCommentsAccessRequirement());
                    });

                options.AddPolicy(
                    AuthorizationPolicies.CanAddTicketComment,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.AddRequirements(
                            new TicketCommentWriteRequirement());
                    });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy(RateLimitingPolicies.Auth, httpContext =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ipAddress ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
            });

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Support Ticket API",
                    Version = "v1",
                    Description = "API for managing support tickets and user authentication."
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token only. Example: eyJhbGciOiJIUzI1NiIs..."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IUserDataAccess, UserDataAccess>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddScoped<ITicketDataAccess, TicketDataAccess>();
            builder.Services.AddScoped<ITicketService, TicketService>();

            builder.Services.AddScoped<IAuditLogDataAccess, AuditLogDataAccess>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();

            builder.Services.AddScoped<IAuthorizationHandler, TicketCommentsAccessHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, TicketCommentWriteHandler>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseRateLimiter();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
