using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Globalization;
using System.Linq.Expressions;

namespace Saga.Persistence.Extensions;

public static class DateFunctions
{
    private static readonly CultureInfo IndonesianCulture = new CultureInfo("id-ID");

    public static ModelBuilder AddDateFunctions(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(() => ToChar(default, default))
            .HasTranslation(args =>
            {
                var dateArg = args.First();
                var formatArg = args.Skip(1).First() as SqlConstantExpression;

                if (formatArg?.TypeMapping == null)
                {
                    throw new InvalidOperationException("The format argument does not have a type mapping assigned.");
                }

                var pgFormat = ConvertToPostgreSqlFormat(formatArg.Value?.ToString() ?? "DD TMMonth YYYY");

                return new SqlFunctionExpression(
                    "to_char",
                    new[]
                    {
                        dateArg,
                        new SqlConstantExpression(
                            Expression.Constant(pgFormat),
                            formatArg.TypeMapping
                        )
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    typeof(string),
                    null
                );
            });

        return modelBuilder;
    }

    public static string ToChar(DateTime date, string format)
        => throw new NotSupportedException();

    public static string ConvertToPostgreSqlFormat(string format)
    {
        return format
            // Day
            .Replace("dd", "DD")     // 01-31
            .Replace("d", "D")       // 1-31
                                     // Month
            .Replace("MMMM", "TMMONTH") // JANUARY-DECEMBER
            .Replace("MMM", "MON")      // JAN-DEC
            .Replace("MM", "MM")        // 01-12
            .Replace("M", "M")          // 1-12
                                        // Year
            .Replace("yyyy", "YYYY")    // 2024
            .Replace("yy", "YY")        // 24
                                        // Custom format for Indonesian month
            .Replace("TMMonth", "TMMONTH");
    }
}
