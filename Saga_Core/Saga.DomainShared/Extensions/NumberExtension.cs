
namespace Saga.DomainShared.Extensions;

public static class NumberExtension
{
    public static string ToCurrencyFormat(this float val, int digit = 3) => val.ToString("n" + digit.ToString());
    public static string ToCurrencyFormat(this double val, int digit = 0) => val.ToString("n" + digit.ToString());
    public static string ToCurrencyFormat(this int val, int digit = 0) => val.ToString("n" + digit.ToString());
    public static string ToCurrencyFormat(this long val, int digit = 0) => val.ToString("n" + digit.ToString());
}
