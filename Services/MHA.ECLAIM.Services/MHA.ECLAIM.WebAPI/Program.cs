using MHA.ECLAIM.Business;
using MHA.ECLAIM.Data.Context;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Microsoft Identity
IEnumerable<string>? initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ');
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
.EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
.AddInMemoryTokenCaches();

// Register PnP Core services and authentication
builder.Services.AddPnPCore();
builder.Services.AddPnPCoreAuthentication();

// Register the JSONAppSettings configuration
builder.Services.Configure<JSONAppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<JSONAppSettings>(builder.Configuration.GetSection("ConnectionStrings"));

// Set the connection string for EF
string encryptedKey = builder.Configuration["AppSettings:AG_ENCKEY"];
string decryptedKey = EncryptionHelper.Decrypt(encryptedKey);

string encryptedConnString = builder.Configuration["ConnectionStrings:dbLogConnString"];
string decryptedConnString = EncryptionHelper.Decrypt(encryptedConnString, decryptedKey);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(decryptedConnString));

// Register Custom services
ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureServices(IServiceCollection services)
{
    //Business Layer
    builder.Services.AddScoped<SearchBL>();
    builder.Services.AddScoped<HomeBL>();
    builder.Services.AddScoped<ClaimRequestBL>();
    builder.Services.AddScoped<ApprovalFormBL>();
}