using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using WebBlogs.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<dbBlogsContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("dbBlogsDemo")));
builder.Services.AddSession();
builder.Services.AddSingleton< HtmlEncoder > (HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));
// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication("CookieAuthentication").AddCookie("CookieAuthentication", config =>
{
    config.Cookie.Name = "UserLoginCookie";
    config.ExpireTimeSpan = TimeSpan.FromDays(1);
    config.LoginPath = "/dang-nhap.html";
    config.LogoutPath = "/dang-xuat.html";
    config.AccessDeniedPath = "/not-found.html";
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Expiration = TimeSpan.FromDays(150);
    options.ExpireTimeSpan = new TimeSpan(150);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"

    );
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
/*app.MapControllerRoute(name: "area",
                pattern: "Admin{*admin}",
                defaults: new { controller = "Home", action = "Index" });
app.MapControllerRoute(name: "default",
               pattern: "{controller=Home}/{action=Index}/{id?}");*/
app.Run();
