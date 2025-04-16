namespace Saga.Infrastructure.Extensions;

public static class EnumExtension
{
    public static IEnumerable<T> ToEnumerableOf<T>(this Enum e)
        => Enum.GetValues(e.GetType()).Cast<T>();
}
