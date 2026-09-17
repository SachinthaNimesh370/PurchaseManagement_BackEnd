using System.Text;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PurchaseManagement.Api.Data;
using PurchaseManagement.Api.Repositories;
using PurchaseManagement.Api.Services;

// Load environment variables from .env file (searches current and parent folders)
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 4));

var builder = WebApplication.CreateBuilder(args);

// Read base connection string from configuration
var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Database=PurchaseManagementDB;TrustServerCertificate=True;";

// Read credentials and overrides from environment variables (.env)
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbTrustCert = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE");

var connectionStringBuilder = new SqlConnectionStringBuilder(baseConnectionString);

if (!string.IsNullOrWhiteSpace(dbServer)) connectionStringBuilder.DataSource = dbServer;
if (!string.IsNullOrWhiteSpace(dbName)) connectionStringBuilder.InitialCatalog = dbName;
if (!string.IsNullOrWhiteSpace(dbUser)) connectionStringBuilder.UserID = dbUser;
if (!string.IsNullOrWhiteSpace(dbPassword)) connectionStringBuilder.Password = dbPassword;
if (bool.TryParse(dbTrustCert, out var trustCert)) connectionStringBuilder.TrustServerCertificate = trustCert;

var connectionString = connectionStringBuilder.ConnectionString;
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// 1. Configure EF Core DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? "EnhanzerSecretKeyForJwtAuthentication2026_LongSecureKey!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? "PurchaseManagementApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? "PurchaseManagementClient";

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    };
});

// 3. Configure CORS for Angular Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 4. Register Services & Repositories
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ILocationService, LocationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 5. Configure Swagger with JWT Bearer Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Purchase Management API",
        Version = "v1",
        Description = "Enhanzer Purchase Management System Backend API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

var app = builder.Build();

// Ensure database table creation if database exists
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        logger.LogInformation("Database verified and schema initialized.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not automatically initialize database table on startup. Ensure SQL Server is reachable.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS before Authentication
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
