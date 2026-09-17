using dotenv.net;
using Microsoft.Data.SqlClient;

// Load environment variables from .env file (searches current and parent directories)
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 4));

var builder = WebApplication.CreateBuilder(args);

// Read base connection string from appsettings.json or fallback
var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Database=PurchaseManagementDB;TrustServerCertificate=True;";

// Read credentials and overrides from environment variables (.env)
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbTrustCert = Environment.GetEnvironmentVariable("DB_TRUST_SERVER_CERTIFICATE");

var connectionStringBuilder = new SqlConnectionStringBuilder(baseConnectionString);

if (!string.IsNullOrWhiteSpace(dbServer))
{
    connectionStringBuilder.DataSource = dbServer;
}
if (!string.IsNullOrWhiteSpace(dbName))
{
    connectionStringBuilder.InitialCatalog = dbName;
}
if (!string.IsNullOrWhiteSpace(dbUser))
{
    connectionStringBuilder.UserID = dbUser;
}
if (!string.IsNullOrWhiteSpace(dbPassword))
{
    connectionStringBuilder.Password = dbPassword;
}
if (bool.TryParse(dbTrustCert, out var trustCert))
{
    connectionStringBuilder.TrustServerCertificate = trustCert;
}

var connectionString = connectionStringBuilder.ConnectionString;

// Set the complete connection string so EF Core / Dapper / DbContext can access it via Configuration
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
