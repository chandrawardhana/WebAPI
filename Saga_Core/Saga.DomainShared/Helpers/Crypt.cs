
using System.ComponentModel;

namespace Saga.DomainShared.Helpers;

public static class Crypt
{
    public static string Encrypt(string _text)
    {
        if (string.IsNullOrEmpty(_text))
            return string.Empty;
        byte[] _textByte = System.Text.ASCIIEncoding.ASCII.GetBytes(_text);
        return System.Convert.ToBase64String(_textByte);
    }

    public static string Decrypt(string _text)
    {
        if (string.IsNullOrEmpty(_text))
            return string.Empty;
        byte[] _textByte = System.Convert.FromBase64String(_text);
        return System.Text.ASCIIEncoding.ASCII.GetString(_textByte);
    }

    public static bool TryDecrypt(string _text, out string value)
    {
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(string));
        try
        {
            value = (string)converter.ConvertFromString(Decrypt(_text));
            return true;
        }
        catch
        {
            value = default(string);
            return false;
        }
    }
}
