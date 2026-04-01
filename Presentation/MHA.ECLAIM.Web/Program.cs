using MHA.ECLAIM.Data;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using MHA.ECLAIM.Process;
using MHA.ECLAIM.Process.Interface;
using MHA.ECLAIM.Web.Components;
using ElmahCore.Mvc;
using ElmahCore.Sql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Identity.Web;
using Radzen;
using LogHelper = MHA.ECLAIM.Framework.Helpers.LogHelper;
using WebApplication = Microsoft.AspNetCore.Builder.WebApplication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Temporary display development error under console log
builder.Services.Configure<CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

// Register Microsoft Identity
IEnumerable<string>? initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ');
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
.EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
.AddInMemoryTokenCaches();

builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Cookie.Name = "EClaimAuth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
    }
);

// Register PnP Core services and authentication
builder.Services.AddPnPCore();
builder.Services.AddPnPCoreAuthentication();

// Register the default HttpClient
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews();

// Register ElmahCore service
builder.Services.AddElmah<SqlErrorLog>(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("elmah-express");
});

// Register the JSONAppSettings configuration
builder.Services.Configure<JSONAppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<JSONAppSettings>(builder.Configuration.GetSection("ConnectionStrings"));

// Register our Custom services
ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseAntiforgery();

// Add ELMAH middleware 
app.UseElmah();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<AuthState>();
    services.AddScoped<DialogService>();

    // Constant 
    services.AddScoped<ConstantHelper>();

    // Helpers
    services.AddScoped<NavigationHelper>();
    services.AddScoped<WorkflowHelper>();
    services.AddScoped<TokenHelper>();

    services.AddScoped<ProjectHelper>();
    services.AddScoped<SharePointHelper>();
    services.AddScoped<ConnectionStringHelper>();
    services.AddScoped<LogHelper>();
    services.AddScoped<TempMessageService>();
    services.AddScoped<PeoplePickerHelper>();
    services.AddScoped<SQLSortingHelper>();
    services.AddScoped<TempMessageService>();

    // Process Layer
    services.AddScoped<IHomeProcess, HomeProcess>();
    services.AddScoped<IClaimProcess, ClaimProcess>();
    services.AddScoped<IWorkflowProcess, WorkflowProcess>();
    services.AddScoped<IReportProcess, ReportProcess>();
    services.AddScoped<IAdministrationProcess, AdministrationProcess>();
    services.AddScoped<IApprovalProcess, ApprovalProcess>();
   
    //Auto Mapper
    services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
}