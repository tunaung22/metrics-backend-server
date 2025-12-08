using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Serilog;

using Metrics.Shared.Utils;
using Metrics.Shared.Configurations;
using Metrics.Infrastructure.Data;
using Metrics.Application.Domains;
using Metrics.Web.Middleware;
using Metrics.Application.Exceptions;
using Metrics.Infrastructure.Data.Seedings;
using Metrics.Application.DTOs.SeedingDtos;
using System.Reflection;
using System.Diagnostics;
using Metrics.Web.Models;
using System.Text.Json.Serialization;
using Metrics.Infrastructure;
using Metrics.Application.Authorization;


// ========== Load .env ===========================================
{
    var dotenv = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    DotenvLoader.Load(dotenv);
}

// ========== Serilog ================
{
    var enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"log-{enviroment}.txt");

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Is(enviroment == "Production" ?
            Serilog.Events.LogEventLevel.Fatal :
            Serilog.Events.LogEventLevel.Debug)
        .WriteTo.Console()
        // .WriteTo.Conditional(
        //     e => enviroment == "Production",
        //     w => w.File(
        //         logFilePath,
        //         buffered: true,
        //         rollingInterval: RollingInterval.Day,
        //         fileSizeLimitBytes: 100 * 1024 * 1024, // 100 MB
        //         rollOnFileSizeLimit: true, // This ensures a new file is created when the limit is reached
        //         restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
        //     )
        // )
        .CreateLogger();
}


// ========== BUILDER ===========================
var builder = WebApplication.CreateBuilder(args);
{
    // ---------- Retrieve Version Number ----------
    // Get the current assembly
    var assembly = Assembly.GetExecutingAssembly();
    var versionInfo = new VersionInfo
    {
        AssemblyVersion = assembly.GetName().Version,
        FileVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion,
        InformationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
    };
    builder.Services.AddSingleton(versionInfo);

    // ---------- Serilog ----------
    builder.Host.UseSerilog();

    /* Configuration loading orders: 
        1) command-line args -> 2) enviroment variables -> 3) user secrets ->
        4) appsettings.{Environment}.json -> 5) appsettings.json -> 
        6) build-in config providers -> 7) custom config
    */
    // ---------- Route Options --------------------
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
        o.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        o.SerializerOptions.WriteIndented = true;
    });


    // TODO: Test only  (to remove)
    // var securitySection = builder.Configuration.GetSection("Security");
    // var jwtSettings = securitySection.GetSection("JwtSettings");
    // Console.WriteLine("JWT SECRET: " + jwtSettings["SecretKey"]);

    // ========== PostgreSQL =======================================================
    // ---------- Read from appsettings.json -> DatabaseSettings.PostgresDbConfig
    var pgConfig = builder.Configuration
        .GetSection("DatabaseSettings:PostgresDbConfig")
        .Get<PostgresDbConfig>()
        ?? throw new MetricsInvalidConfigurationException(
            "PostgresDbConfig section is not configured properly.");

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
            // options.User.RequireUniqueEmail = true;
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

    // ========== Authorization ====================
    builder.Services.AddAuthorization();
    builder.Services
        .AddAuthorizationPolicies()
        .AddAuthorizationHandlers();

    // ========== CONTROLLER, RAZOR PAGES ====================
    builder.Services.AddRazorPages(options =>
    {
        options.Conventions
            .AuthorizeFolder("/Manage", ApplicationPolicies.CanAccess_AdminFeatures_Policy)
            .AuthorizeFolder("/Reports", ApplicationPolicies.CanAccess_AdminFeatures_Policy)
        // .AuthorizeFolder("/Submissions/DepartmentScores", ApplicationPolicies.CanSubmitKpiScorePolicy)
        // .AuthorizeFolder("/Submissions/DepartmentMetricScores", ApplicationPolicies.CanSubmitKeyKpiScorePolicy)
        // .AuthorizeFolder("/Submissions/DepartmentCaseFeedbackScores", ApplicationPolicies.CanSubmitFeedbackScorePolicy)
        // .AuthorizeFolder("/Submissions/DepartmentCaseFeedback", ApplicationPolicies.CanGiveFeedbackPolicy)
        ;
    });
    // builder.Services.AddControllersWithViews();
    builder.Services.AddControllers();

    // ----- Session -----
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromSeconds(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    builder.Services.AddOpenApi();

    // ========== Exception Handling ==============================
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    // ========== Register services and repositories ==============
    builder.Services.AddInfrastructure();
}

// ========== APPLICATION ==========
var app = builder.Build();
{
    app.UseSerilogRequestLogging();

    using (var scope = app.Services.CreateScope())
    {
        Console.WriteLine($"========== Environment: {app.Environment.EnvironmentName} ==========");
        Console.WriteLine($"========== Args received: [{string.Join(", ", args)}] ==========");

        // ----- CHECK CONNECTION -----
        var context = scope.ServiceProvider.GetRequiredService<MetricsDbContext>();

        try
        {
            // Ensure the database is created
            // await context.Database.EnsureCreatedAsync();
            // Attempt to open a connection to the database
            Console.WriteLine("========== Testing database connection... ==========");
            await context.Database.OpenConnectionAsync();
            Console.WriteLine("========== Database connection successful. ==========");

            // ----- RUN DB MIGRATION -----
            Console.WriteLine("========== Checking Database Schema Migrations... ==========");
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (args.Length == 1 && args.Contains("migratedb"))
            {
                if (pendingMigrations.Any())
                {
                    Console.WriteLine("========== INFO: Running pending database migrations... ==========");
                    foreach (var migration in pendingMigrations)
                        Console.WriteLine("### " + migration);
                    // Ensure the database is created
                    // await context.Database.EnsureCreatedAsync();
                    // Run migrations
                    context.Database.Migrate();
                    Console.WriteLine("========== Database migration successful. =====");
                    Console.WriteLine("===============================================");
                }
                else
                    Console.WriteLine("========== No database migration needs to run. =================");

                Console.WriteLine("========== Database Schema Migration complete. Exiting. ========");
                Environment.Exit(0);
            }
            else
            {
                if (pendingMigrations.Any()) // don't run migration, but show message if have pending
                {
                    Console.WriteLine("========== WARNING: You have pending migrations not run yet! ========");
                    Console.WriteLine("========== Use 'migratedb' to run the migration. ====================");
                    foreach (var migration in pendingMigrations)
                        Console.WriteLine("   - " + migration);
                    Console.WriteLine("=====================================================================");
                    Console.WriteLine("========== Pending migrations detected. Exiting. ====================");
                    Environment.Exit(1);
                }
                else
                    Console.WriteLine("========== No database migration needs to run. =============");
            }

            // ----- DATA SEEDING -----
            // ----- Run Identity Seeding -----
            // if (args.Length == 1 && args[0].ToLower() == "seed") { var initialSeedingDataConfig = builder.Configuration.GetSection("InitialSeedingData").Get<InitialSeedingDataConfig>() ?? throw new MetricsInvalidConfigurationException("InitialSeedingData > DefaultUserData section is not configured properly. Check your configuration files."); await InitialUserSeeder.InitAsync(scope.ServiceProvider, initialSeedingDataConfig); }
            Console.WriteLine("========== Checking for sysadmin user... ==========");
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var sysadminUser = await userManager.FindByNameAsync("sysadmin");

            if (args.Length == 1 && args.Contains("inituser"))
            {
                if (sysadminUser != null)
                {
                    Console.WriteLine("========== INFO: There is sysadmin user already! ========");
                    Console.WriteLine("========== InitUser mode complete. Exiting. =============");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("========== INFO: Creating inital user: sysadmin ==========");
                    string? passwordInput;
                    string? confirmPasswordInput;
                    while (true)
                    {
                        passwordInput = ConsoleUtils.ReadPassword("Enter password for sysadmin: ");
                        confirmPasswordInput = ConsoleUtils.ReadPassword("Confirm password: ");
                        if (!passwordInput.Equals(confirmPasswordInput))
                            Console.WriteLine("Password does not match. Please try again.");
                        else
                            break;
                    }
                    var seedUserCreateDto = new SeedUserCreateDto()
                    {
                        Username = "sysadmin",
                        Email = "sysadmin@metricshrm.com",
                        Password = passwordInput ?? "00000000",
                        UserTitleName = "Staff",
                        RolesList = ["Admin", "Staff"],
                        FullName = "System Admin",
                        DepartmentName = "Admin Department",
                        UserCode = new Guid().ToString(),
                        ContactAddress = "",
                        PhoneNumber = ""
                    };
                    await InitialUserSeeder.InitAsync(scope.ServiceProvider, seedUserCreateDto);
                    Console.WriteLine("========== INFO: sysadmin user created. ==============");
                    Console.WriteLine($"Username: {seedUserCreateDto.Username}");
                    Console.WriteLine($"Email: {seedUserCreateDto.Email}");
                    Console.WriteLine($"User title: {seedUserCreateDto.UserTitleName}");
                    string rolesList = string.Join(", ", seedUserCreateDto.RolesList);
                    Console.WriteLine($"Roles: {rolesList}");
                    Console.WriteLine($"Department: {seedUserCreateDto.DepartmentName}");
                    Console.WriteLine("========== InitUser mode complete. Exiting. ==========");
                    Environment.Exit(0);
                }
            }
            else
            {
                if (sysadminUser == null)
                {
                    Console.WriteLine("========== WARNING: You have no sysadmin user yet! ==========");
                    Console.WriteLine("========== Use 'inituser' to add sysadmin user. =============");
                    // await context.Database.CloseConnectionAsync();
                    // return;
                    Console.WriteLine("========== Cannot start application without sysadmin user. Exiting. ==========");
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine("========== INFO: sysadmin user exist! ==========");
                    Console.WriteLine("========== Database startup checks completed successfully. ==========");
                }
            }
        }
        catch (NpgsqlException ex)
        {
            if (ex.SqlState == "3D000")
            {
                Console.WriteLine("========== ERROR: Database connection failed. ==========");
                Console.WriteLine($"   Details :: {ex.Message}");
                Log.Error(ex, "Startup database operations failed");
            }
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"========== ERROR during startup: {ex.Message} ==========");
            Environment.Exit(1);
        }
        finally
        {
            // Ensure the connection is closed. Let EF Core handle connection cleanup, but ensure it's closed.
            try
            {
                if (context.Database.GetDbConnection().State == System.Data.ConnectionState.Open)
                { await context.Database.CloseConnectionAsync(); }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error closing database connection during startup");
                Environment.Exit(1);
            }
        }
    }

    Console.WriteLine("========== Starting Server... ========== ");

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


    app.Use(async (context, next) =>
    {
        var path = context.Request.Path;
        if (!path.StartsWithSegments("/Account/Manage/Password/Change") && ///ForceChangePassword
            !path.StartsWithSegments("/Account/Login") &&
            !path.StartsWithSegments("/Error"))
        {
            var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
            var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

            if (signInManager.IsSignedIn(context.User))
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null && user.IsPasswordChangeRequired)
                {
                    context.Response.Redirect("/Account/Manage/Password/Change"); ///Account/ForceChangePassword
                    return;
                }
            }
        }

        await next();
    });


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
    //     pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapOpenApi();

    // Register shutdown handler
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("Application is shutting down...");
        Log.CloseAndFlush();
    });

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

}
