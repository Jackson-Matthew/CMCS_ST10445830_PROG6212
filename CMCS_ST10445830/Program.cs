using Microsoft.EntityFrameworkCore;
using CMCS_ST10445830.Data;
using CMCS_ST10445830.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register your DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Authentication with Cookie scheme
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Add Authorization with updated roles to match SQL database
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireLecturer", policy =>
        policy.RequireRole("Lecturer"));
    options.AddPolicy("RequireCoordinator", policy =>
        policy.RequireRole("Academic coordinator")); // Updated to match SQL
    options.AddPolicy("RequireManager", policy =>
        policy.RequireRole("HR")); // Updated to match SQL
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Initialize database with seed data
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        dbContext.Database.EnsureCreated();

        // Only seed if no users exist (to avoid conflicts with your SQL data)
        if (!dbContext.Users.Any())
        {
            await SeedData.Initialize(dbContext);
        }
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();