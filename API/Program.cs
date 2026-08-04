using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using FoodOrderAPI.API.Middleware;
using FoodOrderAPI.Application.Common.Behaviors;
using FoodOrderAPI.Application.Interfaces;
using FoodOrderAPI.Application.Orders.Commands.CreateOrder;
using FoodOrderAPI.Domain.Entities;
using FoodOrderAPI.Infrastructure.Data;
using FoodOrderAPI.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")); 

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "FoodOrderAPI",
            Version = "v1"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Informe o token JWT. Exemplo: Bearer {seu_token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }
        ));

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>();

    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
    {
        throw new InvalidOperationException("A chave JWT (Jwt:Key) não está configurada.");
    }

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ProductionCorsPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    builder.Services.AddAuthorization();

    builder.Services.AddHttpClient();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();

    builder.Services.AddMediatR(cfg => {
        cfg.RegisterServicesFromAssembly(typeof(CreateOrderHandler).Assembly);
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderHandler).Assembly);

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter(policyName: "FixedWindowPolicy", limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";
            
            var errorResponse = new { 
                Error = "Too many requests. Please try again later.",
                RetryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                             ? retryAfter.TotalSeconds.ToString() 
                             : "60"
            };
            
            await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken: token);
        };
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseCors("ProductionCorsPolicy");

    app.UseRateLimiter(); 

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
