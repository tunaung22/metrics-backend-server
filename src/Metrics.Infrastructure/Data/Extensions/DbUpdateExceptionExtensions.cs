using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Metrics.Infrastructure.Data.Extensions;

public static class DbUpdateExceptionExtensions
{
    public static bool IsUniqueConstraintViolation(this DbUpdateException ex)
    {
        // return ex.InnerException switch
        // {
        //     PostgresException pgEx => pgEx.SqlState == "23505",
        //     SqlException sqlEx => sqlEx.Number == 2627,
        //     MySqlException mySqlEx => mySqlEx.Number == 1062,
        //     _ => false
        // };

        return ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505";
    }
}
