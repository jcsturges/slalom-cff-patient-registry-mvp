using Azure.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NgrApi.Data;
using NgrApi.Middleware;
using NgrApi.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var logConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "NGR-API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console();

var appInsightsConnStr = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnStr))
{
    logConfig.WriteTo.ApplicationInsights(appInsightsConnStr, TelemetryConverter.Traces);
}

Log.Logger = logConfig.CreateLogger();

builder.Host.UseSerilog();

// Add Azure Key Vault if configured
if (!string.IsNullOrEmpty(builder.Configuration["KeyVault:VaultUri"]))
{
    var keyVaultUri = new Uri(builder.Configuration["KeyVault:VaultUri"]!);
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}

// Add services to the container
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "NGR API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});

// Configure Authentication with Okta
//
// The OIDC discovery document (.well-known/openid-configuration) and JWKS signing
// keys are fetched once and held in memory by ConfigurationManager. We configure
// this explicitly rather than relying on the implicit default so the caching
// behaviour is visible and easy to tune:
//   AutomaticRefreshInterval – proactive background refresh (every 24 h)
//   RefreshInterval           – minimum gap between forced refreshes triggered by
//                               a key-not-found failure (5 min, the IdentityModel default)
var oktaAuthority = builder.Configuration["Okta:Authority"] ?? string.Empty;
var oidcConfigManager = new ConfigurationManager<OpenIdConnectConfiguration>(
    $"{oktaAuthority}/.well-known/openid-configuration",
    new OpenIdConnectConfigurationRetriever(),
    new HttpDocumentRetriever { RequireHttps = !builder.Environment.IsDevelopment() })
{
    AutomaticRefreshInterval = TimeSpan.FromHours(24),
    RefreshInterval           = TimeSpan.FromMinutes(5),
};

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority             = oktaAuthority;
        options.Audience              = builder.Configuration["Okta:Audience"];
        options.RequireHttpsMetadata  = !builder.Environment.IsDevelopment();
        options.ConfigurationManager  = oidcConfigManager;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            RoleClaimType = "groups"
        };
        options.Events = new()
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error(context.Exception, "JWT authentication FAILED: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("JWT token VALIDATED for {Name}", context.Principal?.Identity?.Name ?? "unknown");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Log.Warning("JWT challenge issued: {Error} {ErrorDesc}", context.Error ?? "none", context.ErrorDescription ?? "none");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var hasToken = !string.IsNullOrEmpty(context.Token) ||
                               context.Request.Headers.ContainsKey("Authorization");
                Log.Information("JWT message received. Has auth header: {HasToken}", hasToken);
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SystemAdmin", policy => policy.RequireRole("SystemAdmin"));
    options.AddPolicy("FoundationAnalyst", policy => policy.RequireRole("FoundationAnalyst", "SystemAdmin"));
    options.AddPolicy("ProgramAdmin", policy => policy.RequireRole("ProgramAdmin", "SystemAdmin"));
    options.AddPolicy("ClinicalUser", policy => policy.RequireRole("ClinicalUser", "ProgramAdmin", "SystemAdmin"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register Application Services
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IEmrMappingService, EmrMappingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProgramService, ProgramService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IPatientFileService, PatientFileService>();
builder.Services.AddScoped<IFormBusinessRulesService, FormBusinessRulesService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IDataExportService, DataExportService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IHelpPageService, HelpPageService>();
builder.Services.AddScoped<IContactRequestService, ContactRequestService>();
builder.Services.AddScoped<IDatabaseLockService, DatabaseLockService>();
builder.Services.AddScoped<IImpersonationService, ImpersonationService>();
builder.Services.AddScoped<IDataFeedService, DataFeedService>();
builder.Services.AddScoped<IMigrationService, MigrationService>();
builder.Services.AddScoped<IMigrationValidationService, MigrationValidationService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Auto-create database and tables in development.
// EnsureCreated() only creates the DB if it doesn't exist — it won't add new tables to an
// existing DB when the model changes. We detect schema drift by probing a recently-added table
// and drop+recreate if it's missing so dev never needs manual SQL intervention.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    bool schemaOutOfDate = false;
    try
    {
        dbContext.Database.EnsureCreated();
        // Probe the most recently added table; throws if missing (Epic 13 added MigrationRuns).
        _ = await dbContext.MigrationRuns.AnyAsync();
    }
    catch
    {
        schemaOutOfDate = true;
    }

    if (schemaOutOfDate)
    {
        Log.Information("Dev schema out of date — dropping and recreating database");
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    var emrMappingService = scope.ServiceProvider.GetRequiredService<IEmrMappingService>();
    await emrMappingService.EnsureDefaultMappingsAsync();

    var feedService = scope.ServiceProvider.GetRequiredService<IDataFeedService>();
    await feedService.EnsureDefaultMappingsAsync("system");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowWebApp");

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseMiddleware<ImpersonationMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint — probes DB connectivity (12-006)
app.MapGet("/health", async (NgrApi.Data.ApplicationDbContext db) =>
{
    bool dbOk;
    long dbMs;
    try
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await db.Database.ExecuteSqlRawAsync("SELECT 1");
        sw.Stop();
        dbOk = true;
        dbMs = sw.ElapsedMilliseconds;
    }
    catch
    {
        dbOk = false;
        dbMs = -1;
    }

    var status = dbOk ? "healthy" : "degraded";
    return dbOk
        ? Results.Ok(new { status, timestamp = DateTime.UtcNow, database = new { ok = dbOk, latencyMs = dbMs } })
        : Results.Json(new { status, timestamp = DateTime.UtcNow, database = new { ok = dbOk, latencyMs = dbMs } }, statusCode: 503);
})
.AllowAnonymous();

try
{
    Log.Information("Starting NGR API");
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
