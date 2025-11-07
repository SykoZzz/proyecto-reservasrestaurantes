using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using appReservas.Data; // ✅ Asegúrate de importar Data
using StackExchange.Redis;
using System.Security.Authentication;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using appReservas.Models;
using Microsoft.AspNetCore.Identity.UI.Services; // ✅ Importar IEmailSender
using PROYECTO_RESERVASRESTAURANTES.Integration.galletafortuna; 
using Stripe;
using appReservas.Models;

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
// 🔹 CONFIGURACIÓN IDENTITY CON ROLES
// ======================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ======================================================
// 🔹 REGISTRAR DummyEmailSender SIMPLIFICADO
// ======================================================
builder.Services.AddSingleton<IEmailSender, DummyEmailSender>(); // ✅ Ya no depende de ILogger

// ======================================================
// 🔹 REDIRECCIÓN AUTOMÁTICA SEGÚN ROL
// ======================================================
builder.Services.ConfigureApplicationCookie(options =>
{

    // 🔧 Corrección: Identity usa /Identity/Account/Login, no /Account/Login
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";


    options.Events.OnSigningIn = async context =>
    {
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.Principal);

        if (user != null)
        {
            if (await userManager.IsInRoleAsync(user, "Administrador"))
                context.Properties.RedirectUri = "/Admin/Reservas";
            else if (await userManager.IsInRoleAsync(user, "Empleado"))
                context.Properties.RedirectUri = "/Reservas/MisReservas";
        }
    };
});

// ======================================================
// 🔹 REDIS CONFIG
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
// 🔹 SESSION
// ======================================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient<GalletaApiIntegration>(); // HttpClient para consumir API externa de RapidAPI
builder.Services.AddHttpClient();

// ======================================================
// 🔹 MVC, Razor y Servicios Auxiliares
// ======================================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

var stripeSettings = builder.Configuration.GetSection("Stripe").Get<StripeSettings>();
if (stripeSettings != null && !string.IsNullOrEmpty(stripeSettings.SecretKey))
{
    StripeConfiguration.ApiKey = stripeSettings.SecretKey;
}

var app = builder.Build();

// ======================================================
// 🔹 MIGRACIONES AUTOMÁTICAS (solo en desarrollo)
// ======================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    if (!app.Environment.IsProduction())
    {
        db.Database.Migrate();
    }
    else
    {
        try
        {
            if (!db.Database.CanConnect())
                Console.WriteLine("⚠️ No se pudo conectar a la base de datos Neon.");
            else
                Console.WriteLine("✅ Conectado a la base de datos Neon existente (sin aplicar migraciones).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al conectar a la base de datos: {ex.Message}");
        }
    }
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
