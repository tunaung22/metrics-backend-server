using Metrics.Application.Services.IServices;
using Metrics.Application.Services;
using Metrics.Common.Configurations;
using Metrics.Common.Utils;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Metrics.Infrastructure;
using Metrics.Infrastructure.Repositories;
using Metrics.Infrastructure.Repositories.IRepositories;
using Metrics.Infrastructure.Data;
using Metrics.Domain.Entities;
using Metrics.Web.Exceptions;
using Serilog;


// ========== Load .env =========================================================== 
var dotenv = Path.Combine(Directory.GetCurrentDirectory(), ".env");
DotenvLoader.Load(dotenv);

// ========== Serilog ==================================================
var log = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
Log.Logger = log;


// ========== BUILDER =============================================================
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


var jwtSettings = builder.Configuration.GetSection("DatabaseSettings");
var pgDb = jwtSettings.GetSection("PostgresDb");
Console.WriteLine("JWT SECRET: " + pgDb["PgSchema"]);




// ========== PostgreSQL =======================================================
var pgConfig = builder.Configuration
    .GetSection("DatabaseSettings:PostgresDb")
    .Get<PostgresDbConfig>()
    ?? throw new InvalidOperationException("PostgresDb section is not configured properly.");

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


// ========== DbContext ========================================================
try
{
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
    });

}
catch (Exception e)
{
    Console.WriteLine("essfsdsfsdfsfd;', ", e);
}



// ========== Identity =========================================================
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<MetricsDbContext>()
    .AddDefaultTokenProviders();
//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<MetricsDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
});

// ========== CONTROLLER, RAZOR PAGES===========================================
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/manage", "kpi/");
    options.Conventions.AddPageRoute("/manage", "departments/");
});
builder.Services.AddControllers();

builder.Services.AddOpenApi();

// ========== Register services and repositories ===============================
// builder.Services.AddScoped<>();
// ===== Unit of Work =======
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ===== Repository =========
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IKpiPeriodRepository, KpiPeriodRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IKpiSubmissionRepository, KpiSubmissionRepository>();
// ===== Service ============
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IKpiPeriodService, KpiPeriodService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<IKpiSubmissionService, KpiSubmissionService>();

// ========== Exception Handling ===============================================
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


// ========== APPLICATION ======================================================
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

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

app.UseStatusCodePagesWithReExecute("/errors/{0}");
app.UseStatusCodePages();

app.Run();
