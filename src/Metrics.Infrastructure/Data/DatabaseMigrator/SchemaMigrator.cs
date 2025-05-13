using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Data.DatabaseMigrator;

public static class SchemaMigrator
{
    public static async Task MigrateDbAsync(string[] args, MetricsDbContext context)
    {
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            if (args.Contains("migrate-db"))
            {
                // migrate
                try
                {
                    await context.Database.MigrateAsync();
                    Console.WriteLine("========== Database migration task finished. ==========");
                    Console.WriteLine($"========== {pendingMigrations.Count()} migrations applied. ==========");
                }
                catch (Exception e)
                {
                    throw new Exception("========= WARNING: Database migration task have failed! =========", e);
                }
            }
            else
            {
                // no migrate
                // but show message if have pending
                Console.WriteLine("========== WARNING: You have pending migrations not run yet! ==========");
                Console.WriteLine("========== Use dotnet run migrate-db to run the migration again. ==========");
                foreach (var migration in pendingMigrations)
                    Console.WriteLine("### " + migration);
            }
        }
        else
        {
            Console.WriteLine("========== No database migration needs to run. ==========");
        }

    }
}
