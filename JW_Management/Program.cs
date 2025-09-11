using System.Globalization;
using System.Text;
using JW_Management.Component.DB;
using JW_Management.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using LettuceEncrypt;
using Microsoft.AspNetCore.DataProtection;
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
builder.Services.AddScoped<FileContext>();

var _dbBackupService = new MySqlBackupService(new JW_Management.Models.AppContext(tenantContext));
var dbService = new DbService(new JW_Management.Models.AppContext(tenantContext));
builder.Configuration["DBVersion"] = dbService._version;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptions =>
{
    cookieOptions.LoginPath = "/Home/Login";
    cookieOptions.AccessDeniedPath = "/Home/Error";
});

#if !DEBUG
//builder.Services.AddDataProtection().SetApplicationName("JW_Management").PersistKeysToFileSystem(new DirectoryInfo("/https/"));
//builder.Services.AddLettuceEncrypt().PersistDataToDirectory(new DirectoryInfo("/https/"), "Mon2020@");
#endif

var app = builder.Build();

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
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.None,
};
app.UseCookiePolicy(cookiePolicyOptions);
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
