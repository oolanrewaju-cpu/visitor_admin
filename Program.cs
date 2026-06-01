using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Xml.Linq;
using visitor_admin.Entities;
using visitor_admin.Helpers;
using visitor_admin.Repositories.Implementations;
using visitor_admin.Repositories.Interfaces;
using visitor_admin.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try {
    Log.Information("Starting up the service...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();
    // Add services to the container.

    builder.Services.AddScoped<IStaffRepository, StaffRepository>();
    builder.Services.AddScoped<IRequestRoleRepository, RequestRoleRepository>();
    builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
    builder.Services.AddScoped<IOtpVerificationRepository, OtpVerificationRepository>();
    builder.Services.AddScoped<JwtService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IOtpService, OtpService>();
    builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
    builder.Services.AddControllers();

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // Define the Bearer auth scheme
        options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Input a valid token to access this API"
        });

        // Require the Bearer token globally across all endpoints
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        });
    });
    builder.Services.AddOpenApi();

    var jwtSecretKey = builder.Configuration["Authentication:SecretForKey"] ?? throw new InvalidOperationException("JWT secret key is not configured");
    var jwtIssuer = builder.Configuration["Authentication:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured");
    var jwtAudience = builder.Configuration["Authentication:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured");

    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
            };
        });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var existingAdmin = await userRepository.GetByUsernameAsync("admin");
        if (existingAdmin == null)
        {
            var admin = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = PasswordHelper.Hash("Admin@123"),
                FirstName = "System",
                LastName = "Admin",
                Role = "Admin",
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            await userRepository.CreateAsync(admin);
            Log.Information("Default admin user seeded: admin / Admin@123");
        }
    }

    app.UseSerilogRequestLogging(options =>
    {
        // Customize the message template
        options.MessageTemplate = "Handled {RequestPath}";

        // Emit debug-level events instead of the defaults
        options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

        // Attach additional properties to the request completion event
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options => {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty; 
        });
    }

    app.UseHttpsRedirection();

    app.UseCors();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}