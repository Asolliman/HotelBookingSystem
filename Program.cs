// Program.cs
using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.SQLite;
using HotelBookingAPI.Data;
using HotelBookingAPI.Helpers;
using HotelBookingAPI.Jobs;
using HotelBookingAPI.Models.Entities;
using HotelBookingAPI.Services.Implementations;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 3. Add JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "YourSuperSecretKeyHereMakeItLong1234567890";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "HotelBookingAPI";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "HotelBookingClient";
var jwtExpiresInMinutes = int.Parse(builder.Configuration["JwtSettings:ExpiresInMinutes"] ?? "60");

var key = Encoding.ASCII.GetBytes(jwtSecret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// 4. Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ReceptionistOrAdmin", policy => policy.RequireRole("Admin", "Receptionist"));
    options.AddPolicy("GuestOrHigher", policy => policy.RequireRole("Admin", "Receptionist", "Guest"));
});

// 5. Register all services via DI
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IGuestProfileService, GuestProfileService>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<HangfireJobService>();

// 6. Add Hangfire with SQLite storage
builder.Services.AddHangfire(config =>
    config.UseSQLiteStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();

// 7. Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// 8. Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 9. Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    try
    {
        // Create the database and schema
        await context.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        // Log and continue if database creation fails
        Console.WriteLine($"Database creation attempted: {ex.Message}");
    }
    
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seeding failed: {ex.Message}");
    }
}

// 10. Register Hangfire recurring job
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var jobService = scope.ServiceProvider.GetRequiredService<HangfireJobService>();
    try
    {
        recurringJobManager.AddOrUpdate<HangfireJobService>(
            "CancelUnpaidReservations",
            job => job.CancelUnpaidReservationsAsync(),
            Cron.Hourly);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hangfire job registration failed: {ex.Message}");
    }
}

// 11. Map Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.Run();

// Seeding helper
public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "Admin", "Receptionist", "Guest" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create admin user
        var adminUser = new ApplicationUser
        {
            UserName = "admin@hotel.com",
            Email = "admin@hotel.com",
            FullName = "Admin User",
            CreatedAt = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

// Hangfire authorization filter
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.IsInRole("Admin");
    }
}
