
using System.Reflection;

namespace Saga.Infrastructure.Constants;

public static class PathDirectory
{
    public static string Resources => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? String.Empty,
            nameof(Resources)
        );

    public static string TempReports => Path.Combine(Resources, nameof(TempReports));
    public static string TempCVReports => Path.Combine(Resources, nameof(TempCVReports));
    public static string Logo => Path.Combine(Resources, nameof(Logo), "SidoAgung.png");
}
