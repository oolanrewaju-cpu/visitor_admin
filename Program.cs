// imported packages as well as classes from other classes in the project
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

// logger configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try {
    Log.Information("Starting up the service...");
    // This sets up and configures everything the application needs to run.
    var builder = WebApplication.CreateBuilder(args);
        
    // replaces ASP.NET Core's default logging system with Serilog as the logging provider for the entire application
    builder.Host.UseSerilog();
    
    // Add services to the container.
    // These are custom services injected into the app.
    // These tell the app what classes to use when services are requested.
    builder.Services.AddScoped<IStaffRepository, StaffRepository>();
    builder.Services.AddScoped<IRequestRoleRepository, RequestRoleRepository>();
    builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
    builder.Services.AddScoped<IOtpVerificationRepository, OtpVerificationRepository>();
    builder.Services.AddScoped<JwtService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IOtpService, OtpService>();
    
    // registers automapper and tells it to scan the project for all mapping profiles automatically
    builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly)); 
    
    // registers MVC controller system so that app can handle HTTP requests through controllers
    builder.Services.AddControllers();
    
    // This sets the maximum file upload size in the app to 5mb
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 5 * 1024 * 1024;
    });
    // Ensures all endpoints require authentication by default unless explicitly marked with [AllowAnonymous]
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });
    // Enables swagger to discover and document the API endpoints
    builder.Services.AddEndpointsApiExplorer();
    // Configures Swagger to support JWT Authentication, adding a bearer token input field to the swagger UI
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
    //Registers Microsoft's built-in OpenAPI document generation service, similar to Swagger but native to ASP.NET Core.
    builder.Services.AddOpenApi();

    //  Extracting secrets from the config file. Will be environment variables in production.
    var jwtSecretKey = builder.Configuration["Authentication:SecretForKey"] ?? throw new InvalidOperationException("JWT secret key is not configured");
    var jwtIssuer = builder.Configuration["Authentication:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured");
    var jwtAudience = builder.Configuration["Authentication:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google"; // let middleware handle this, not your controller
        options.SaveTokens = true;
    })
    .AddJwtBearer(options =>
        {            options.TokenValidationParameters = new TokenValidationParameters
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

    //builder.Services.AddAuthentication().AddGoogleOpenIdConnect(googleOptions =>
    //{
    //    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId is not configured"); 
    //    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret is not configured");
    //});

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    var app = builder.Build();
    
    app.UseCors();

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