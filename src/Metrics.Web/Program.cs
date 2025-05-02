using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Serilog;

using Metrics.Shared.Utils;
using Metrics.Shared.Configurations;
using Metrics.Infrastructure.Data;
using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Repositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Services;
using Metrics.Web.Middleware;
using Metrics.Application.Exceptions;
using Metrics.Infrastructure.Data.Seedings;
using Microsoft.AspNetCore.Authorization;


// ========== Load .env ===========================================
var dotenv = Path.Combine(Directory.GetCurrentDirectory(), ".env");
DotenvLoader.Load(dotenv);

// ========== Serilog ================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


// ========== BUILDER ===========================
var builder = WebApplication.CreateBuilder(args);

/* Configuration loading orders: 
    1) command-line args -> 2) enviroment variables -> 3) user secrets ->
    4) appsettings.{Environment}.json -> 5) appsettings.json -> 
    6) build-in config providers -> 7) custom config
*/
builder.Services.Configure<RouteOptions>(o =>
{
    o.LowercaseUrls = true;
    o.LowercaseQueryStrings = true;
    o.AppendTrailingSlash = true;
});

builder.Services.ConfigureHttpJsonOptions(o =>
{
    // o.SerializerOptions.AllowTrailingCommas = true;
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
});


// TODO: Test only  (to remove)
var securitySection = builder.Configuration.GetSection("Security");
var jwtSettings = securitySection.GetSection("JwtSettings");
// Console.WriteLine("JWT SECRET: " + jwtSettings["SecretKey"]);




// ========== PostgreSQL ===========
var pgConfig = builder.Configuration
    .GetSection("DatabaseSettings:PostgresDbConfig")
    .Get<PostgresDbConfig>()
    ?? throw new MetricsInvalidConfigurationException("PostgresDbConfig section is not configured properly.");

builder.Services.Configure<PostgresDbConfig>(options =>
{
    options.PgSchema = pgConfig.PgSchema;
    // options.PgPoolSize = ...,
});

var connectionString = new NpgsqlConnectionStringBuilder()
{
    Host = builder.Configuration["PG_HOST"],
    Port = int.Parse(builder.Configuration["PG_PORT"] ?? "5432"),
    Database = builder.Configuration["PG_DB_NAME"],
    Username = builder.Configuration["PG_DB_USER"],
    Password = builder.Configuration["PG_DB_PASSWORD"],
    // SearchPath = pgSchema,
}.ConnectionString;


// ========== DbContext =============================
// Register DbContext with Scoped lieftime by default
builder.Services.AddDbContext<MetricsDbContext>(options =>
{
    // options.UseNpgsql(builder.Configuration.GetConnectionString());
    options
        .UseNpgsql(connectionString, o =>
            {
                o.MigrationsAssembly("Metrics.Web");
                o.MigrationsHistoryTable("__ef_migrations_history", pgConfig.PgSchema);
            }
        )
        .UseSnakeCaseNamingConvention()
        .LogTo(Console.WriteLine, LogLevel.Information);
}, ServiceLifetime.Scoped);





// ========== Identity ==================================================
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = false;
        options.Password.RequireDigit = false; // Allow passwords without digits
        options.Password.RequireLowercase = false; // Allow passwords without lowercase letters
        options.Password.RequireUppercase = false; // Allow passwords without uppercase letters
        options.Password.RequireNonAlphanumeric = false; // Allow passwords without special characters
        options.Password.RequiredLength = 6;
        // options.Password.RequiredLength = 8; // Set the minimum length to 8
        options.Password.RequiredUniqueChars = 1; // Set the number of unique characters required
    })
    .AddEntityFrameworkStores<MetricsDbContext>()
    .AddDefaultTokenProviders();
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<MetricsDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/error/unauthorized";
});

// ========== CONTROLLER, RAZOR PAGES ==========
builder.Services.AddRazorPages(options =>
{
    // options.Conventions.AddPageRoute("/Kpi/Index", "/manage/kpi");
    // options.Conventions.AddPageRoute("/Departments/Index", "/manage/departments");
});
builder.Services.AddControllers();

// ----- Session -----
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Employee", policy => policy.RequireRole("Employee"));
});


builder.Services.AddOpenApi();

// ========== Register services and repositories ==========
// builder.Services.AddScoped<>();
// ===== Unit of Work =======
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ===== Repository =========
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IKpiPeriodRepository, KpiPeriodRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IKpiSubmissionRepository, KpiSubmissionRepository>();
// ===== Service ============
builder.Services.AddScoped<ISeedingService, SeedingService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IKpiPeriodService, KpiPeriodService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IKpiSubmissionService, KpiSubmissionService>();

// ========== Exception Handling ==============================
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


// ========== APPLICATION ==========
var app = builder.Build();

// ----- DATA SEEDING -----
// ----- Run Identity Seeding -----
// if (args.Length == 1 && args[0].ToLower() == "seed")
// {
using (var scope = app.Services.CreateScope())
{
    // var initialSeedingDataConfig = builder.Configuration
    //     .GetSection("InitialSeedingData")
    //     .Get<InitialSeedingDataConfig>()
    //     ?? throw new MetricsInvalidConfigurationException("InitialSeedingData > DefaultUserData section is not configured properly. Check your configuration files.");
    // await InitialUserSeeder.InitAsync(scope.ServiceProvider, initialSeedingDataConfig);
    await InitialUserSeeder.InitAsync(scope.ServiceProvider);
}
// }


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.UseExceptionHandler("/Error");
    // app.UseStatusCodePagesWithReExecute("/Error/{0}");
    // app.UseStatusCodePages();
    // app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseStatusCodePages();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

app.MapRazorPages();
app.MapControllers();
// app.MapControllerRoute(
//     name: "default",
//     pattern: "app/{controller=Home}/{action=Index}/{id?}")
//     .WithStaticAssets();
//     // pattern: "mv/{controller=Home}/{action=Index}/{periodName?}/{departmentCode}")
//     // pattern: "mvc/{controller=Home}/{action=Index}")
// app.MapControllerRoute(
//     name: "default",
//     pattern: "app/{controller=Home}/{action=Index}/{id?}");

app.MapOpenApi();

app.Run();

// try
// {
//     app.Run();
// }
// catch (Exception ex)
// {
//     // Log the exception
//     // var logger = app.Services.GetRequiredService<ILogger<Program>>();
//     // logger.LogError(ex, "An unhandled exception occurred during application startup");
//     Log.Fatal(ex, "Application terminated unexpectedly");
//     throw;
// }
// finally
// {
//     Log.CloseAndFlush();
// }

