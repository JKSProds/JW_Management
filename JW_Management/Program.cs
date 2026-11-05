using System.Globalization;
using System.Net;
using System.Text;
using JW_Management.Component.DB;
using JW_Management.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using LettuceEncrypt;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using AppContext = System.AppContext;

var accessor = new HttpContextAccessor();
var builder = WebApplication.CreateBuilder(args);

var tenantContext =
    new TenantContext(accessor, builder.Configuration.GetConnectionString("DefaultConnection")!);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(tenantContext);
builder.Services.AddScoped<JW_Management.Models.AppContext>();
builder.Services.AddScoped<DbContext>();
builder.Services.AddScoped<JWApiContext>();
builder.Services.AddSingleton<HourglassContext>();
builder.Services.AddScoped<HourglassImportador>();
builder.Services.AddScoped<HourglassPrograma>();
builder.Services.AddScoped<HourglassSincronizador>();
builder.Services.AddScoped<FileContext>();
builder.Services.AddScoped<MailContext>();

var _dbBackupService = new MySqlBackupService(new JW_Management.Models.AppContext(tenantContext));
var dbService = new DbService(new JW_Management.Models.AppContext(tenantContext));
builder.Configuration["DBVersion"] = dbService._version;

// Configuração global na BD master; sobrepõe user-secrets e variáveis de ambiente. Mesmas chaves/app name do DataProtection abaixo.
#if DEBUG
var earlyProtection = DataProtectionProvider.Create("JW_Management");
#else
var earlyProtection = DataProtectionProvider.Create(new DirectoryInfo("/app/img/https/"), o => o.SetApplicationName("JW_Management"));
#endif
var configStore = new ConfigStore(builder.Configuration.GetConnectionString("DefaultConnection")!, earlyProtection);
configStore.EnsureTable();
builder.Configuration.AddInMemoryCollection(configStore.Load());
builder.Services.AddSingleton(configStore);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    var proxy = builder.Configuration["ForwardedHeaders:KnownProxy"]; // IP do Caddy
    if (!string.IsNullOrEmpty(proxy)) options.KnownProxies.Add(IPAddress.Parse(proxy));
});

var auth = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptions =>
{
    cookieOptions.LoginPath = "/Home/Login";
    cookieOptions.AccessDeniedPath = "/Home/Error";
    cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    cookieOptions.Cookie.HttpOnly = true;
    cookieOptions.SlidingExpiration = true;
    cookieOptions.ExpireTimeSpan = TimeSpan.FromHours(8);
}).AddCookie(OidcLogin.PendingScheme, cookieOptions =>
{
    cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    cookieOptions.Cookie.HttpOnly = true;
    cookieOptions.ExpireTimeSpan = TimeSpan.FromMinutes(5);
});

// Só regista o OIDC se estiver configurado (Oidc:Authority, Oidc:ClientId, Oidc:ClientSecret); caso contrário fica só o login antigo.
var oidc = builder.Configuration.GetSection("Oidc");
if (!string.IsNullOrEmpty(oidc["ClientId"]))
{
    auth.AddOpenIdConnect(options =>
    {
        options.Authority = oidc["Authority"];
        options.ClientId = oidc["ClientId"];
        options.ClientSecret = oidc["ClientSecret"];
        options.ResponseType = "code";
        options.UsePkce = true;
        options.ResponseMode = "query";
        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Events = new OpenIdConnectEvents
        {
            OnTokenValidated = OidcLogin.OnTokenValidated,
            OnRemoteFailure = ctx =>
            {
                ctx.Response.Redirect("/acesso-negado");
                ctx.HandleResponse();
                return Task.CompletedTask;
            }
        };
    });
}

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueCountLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100_000_000;
});

#if !DEBUG
builder.Services.AddDataProtection().SetApplicationName("JW_Management").PersistKeysToFileSystem(new DirectoryInfo("/app/img/https/"));
//builder.Services.AddLettuceEncrypt().PersistDataToDirectory(new DirectoryInfo("/https/"), "Mon2020@");
#endif

var app = builder.Build();

app.UseForwardedHeaders();

//Cultura PT
System.Globalization.CultureInfo cc = new CultureInfo("pt-PT");
cc.NumberFormat.NumberDecimalSeparator = ".";
CultureInfo.DefaultThreadCurrentCulture = cc;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

var cookiePolicyOptions = new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax, // Strict impediria o retorno do Pocket ID
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always,
};
app.UseCookiePolicy(cookiePolicyOptions);
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
