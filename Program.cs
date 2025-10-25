using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using appReservas.Data;
using StackExchange.Redis;
using System.Security.Authentication;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// 🔹 CONFIGURACIÓN BASE DE DATOS (PostgreSQL - Neon)
// ======================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ======================================================
// 🔹 CONFIGURACIÓN IDENTITY Y ROLES
// ======================================================
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ======================================================
// 🔹 REDIRECCIÓN AUTOMÁTICA SEGÚN ROL
// ======================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnSigningIn = async context =>
    {
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.GetUserAsync(context.Principal);

        if (user != null)
        {
            if (await userManager.IsInRoleAsync(user, "Administrador"))
            {
                context.Properties.RedirectUri = "/Admin/Reservas";
            }
            else if (await userManager.IsInRoleAsync(user, "Empleado"))
            {
                context.Properties.RedirectUri = "/Reservas/MisReservas";
            }
        }
    };
});

// ======================================================
// 🔹 REDIS CONFIG (Render Cloud o local)
// ======================================================
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = "localhost:6379";
    });
}
else
{
    var redisConn = builder.Configuration["Redis:ConnectionString"];
    if (!string.IsNullOrEmpty(redisConn))
    {
        var redisOptions = ConfigurationOptions.Parse(redisConn);
        redisOptions.AbortOnConnectFail = false;
        redisOptions.ConnectTimeout = 10000;
        redisOptions.SyncTimeout = 10000;
        redisOptions.KeepAlive = 60;
        redisOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);
        redisOptions.Ssl = true;
        redisOptions.User = "default";            
        redisOptions.Password = "GrIpBLJOfRNZ88ow4vm9m7Ve9YaZNRT5"; 
        redisOptions.SslProtocols = SslProtocols.Tls12;

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = redisOptions;
        });
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
    }
}

// ======================================================
// 🔹 SESSION (usa Redis si está disponible)
// ======================================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ======================================================
// 🔹 MVC, Razor y Servicios Auxiliares
// ======================================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ======================================================
// 🔹 MIGRACIONES AUTOMÁTICAS AL ARRANCAR
// ======================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Si existe clase SeedData en tu proyecto, puedes descomentar esto:
    // await SeedData.EnsureSeedDataAsync(services);
}

// ======================================================
// 🔹 MIDDLEWARES
// ======================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ======================================================
// 🔹 RUTAS PRINCIPALES DEL PROYECTO
// ======================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Reservas}/{action=Index}/{id?}"
);
app.MapRazorPages();

// ======================================================
// 🔹 PUERTO DINÁMICO PARA RENDER / DOCKER
// ======================================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
