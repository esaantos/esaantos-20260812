using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GestaoColaboradores.API.Services.Common;

public class DbExceptionTranslator
{
    public static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: "23505" };

    public static string? GetConstraintName(DbUpdateException ex) =>
        (ex.InnerException as PostgresException)?.ConstraintName;
}