using CMCS_ST10445830.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 1??  Add services to the container
// ------------------------------------------------------------
builder.Services.AddControllersWithViews();

// ? Register our in-memory storage service as a singleton
builder.Services.AddSingleton<IInMemoryStorageService, InMemoryStorageService>();

// ? Add session support (for TempData and user persistence)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ------------------------------------------------------------
// 2??  Build the app
// ------------------------------------------------------------
var app = builder.Build();

// ------------------------------------------------------------
// 3??  Configure the HTTP request pipeline
// ------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ? Enable HTTPS and static files (for uploads and CSS/JS)
app.UseHttpsRedirection();
app.UseStaticFiles();

// ? Standard ASP.NET Core pipeline
app.UseRouting();

// (Optional) — Add authentication/authorization later if needed
app.UseAuthorization();

// ? Enable session usage before MVC endpoints
app.UseSession();

// ------------------------------------------------------------
// 4??  Map controller routes
// ------------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ------------------------------------------------------------
// 5??  Run the app
// ------------------------------------------------------------
app.Run();
