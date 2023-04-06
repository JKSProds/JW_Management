using System.Globalization;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Add(new ServiceDescriptor(typeof(JW_Management.Models.DbContext), new JW_Management.Models.DbContext(builder.Configuration.GetConnectionString("DefaultConnection")!)));

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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
