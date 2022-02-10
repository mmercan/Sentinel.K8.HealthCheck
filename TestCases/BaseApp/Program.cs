using System.IdentityModel.Tokens.Jwt;
using BaseApp.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Events;
using Turquoise.HealthChecks.Common;
using Turquoise.HealthChecks.Common.Checks;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (environment == null) { environment = "Development"; }
var appname = System.AppDomain.CurrentDomain.FriendlyName;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(options =>
        options.ClientCertificateMode = ClientCertificateMode.RequireCertificate);
});

builder.Host.UseSerilog((ctx, lc) => lc
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Enviroment", environment)
    .Enrich.WithProperty("ApplicationName", appname)
    .MinimumLevel.Override("Default", LogEventLevel.Debug)
    .WriteTo.Console()
    .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day)
);

if (builder.Configuration.GetValue<bool>("AzureAd:IsValidationEnabled"))
{
    builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
        .AddInMemoryTokenCaches();
}

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
 .AddSystemInfoCheck()
.AddConfigurationChecker(builder.Configuration);

builder.Services.AddSingleton<ICertificateProvider, CertificateProvider>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.Configure<ValidateCertificateSettings>(builder.Configuration.GetSection("CertValidation"));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

if (builder.Configuration.GetValue<bool>("AzureAd:IsValidationEnabled"))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

if (builder.Configuration.GetValue<bool>("CertValidation:IsValidationEnabled"))
{
    app.UseClientCertificateValidationMiddleware();
}

// if (builder.Configuration.GetValue<bool>("AzureAd:IsValidationEnabled"))
// {
app.UseHealthChecksWithAuth("/Health/IsAliveAndWellDetailsAuth", new HealthCheckOptions() { ResponseWriter = WriteResponses.WriteListResponse });
// }
// else
// {
app.UseHealthChecks("/Health/IsAliveAndWellDetails", new HealthCheckOptions() { ResponseWriter = WriteResponses.WriteListResponse });
// }

app.UseHealthChecks("/Health/IsAliveAndWell");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
