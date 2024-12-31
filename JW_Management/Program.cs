using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using LettuceEncrypt;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Add(new ServiceDescriptor(typeof(JW_Management.Models.DbContext), new JW_Management.Models.DbContext(builder.Configuration.GetConnectionString("DefaultConnection")!)));
builder.Services.Add(new ServiceDescriptor(typeof(JW_Management.Models.JWApi), new JW_Management.Models.JWApi()));
builder.Services.Add(new ServiceDescriptor(typeof(JW_Management.Models.MySqlBackupService), new JW_Management.Models.MySqlBackupService(builder.Configuration.GetConnectionString("DefaultConnection")!, $"/app/img/backup/" )));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptions =>
{
    cookieOptions.LoginPath = "/Home/Login";
    cookieOptions.AccessDeniedPath = "/Home/Error";
});

#if !DEBUG
builder.Services.AddDataProtection().SetApplicationName("JW_Management").PersistKeysToFileSystem(new DirectoryInfo("/https/"));
builder.Services.AddLettuceEncrypt().PersistDataToDirectory(new DirectoryInfo("/https/"), "Mon2020@");
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
