using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using AcerProTask.Data;
using AcerProTask.Services;
using AcerProTask.Models;
using Microsoft.AspNetCore.Identity;

using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day);
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/RegisterAndLogin";
        options.AccessDeniedPath = "/Account/RegisterAndLogin"; 
        options.LogoutPath = "/Account/Logout";
    });


builder.Services.AddAuthorization();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // options.SignIn.RequireConfirmedAccount = false;
    // options.Lockout.AllowedForNewUsers = false;
    // options.SignIn.RequireConfirmedEmail = false;
    // options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient();


builder.Services.AddHostedService<HealthCheckService>();

builder.Services.AddSingleton(smtpClient => new SmtpClient
{
    Host = builder.Configuration["Smtp:Host"],
    Port = int.Parse(builder.Configuration["Smtp:Port"]),
    EnableSsl = bool.Parse(builder.Configuration["Smtp:EnableSsl"]),
    Credentials = new System.Net.NetworkCredential(
        builder.Configuration["Smtp:Username"],
        builder.Configuration["Smtp:Password"]
    )
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); 
app.UseStaticFiles();

app.UseRouting(); 

app.UseAuthentication();
app.UseAuthorization();  

app.UseSession(); 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
