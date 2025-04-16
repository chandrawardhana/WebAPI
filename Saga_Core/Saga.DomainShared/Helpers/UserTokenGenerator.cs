
using Saga.Domain.Entities.Systems;
using System.ComponentModel;
using System.Globalization;

namespace Saga.DomainShared.Helpers;

public static class UserTokenGenerator
{
    public static string GenerateToken(UserProfile profile)
        => Crypt.Encrypt(string.Format("{0:yyyy-MM-dd HH:mm:ss}^_^{1}^_^{2:yyyy-MM-dd HH:mm:ss}", DateTime.Now, profile.Key, DateTime.Now));

    public static Result TokenValidation(UserProfile profile, string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException("Token");

            if (!Crypt.TryDecrypt(token, out var tokenDecrypted))
                throw new Exception("Token's Not Valid:01");

            var splits = tokenDecrypted.Split("^_^");

            // VALIDATE TIME TOKEN
            if (string.IsNullOrEmpty(splits.ElementAtOrDefault(0)))
                throw new Exception("Token's Not Valid:03");

            if (!DateTime.TryParse(splits.ElementAtOrDefault(0), out var date))
                throw new Exception("Token's Not Valid:04");

            if ((DateTime.Now - date).TotalMinutes > 120) // must 5 minute
                throw new Exception("Token's Expired:05");

            // VALIDATE TOKEN USER ID
            if (string.IsNullOrEmpty(splits.ElementAtOrDefault(1)))
                throw new Exception("Token's Not Valid:06");

            if (!Guid.TryParse(splits.ElementAtOrDefault(1), out var id))
                throw new Exception("Token's Not Valid:07");

            if (id != profile.Key)
                throw new Exception("Token's Not Valid:08");

            return Result.Success();

        }
        catch (Exception ex)
        {
            return Result.Failure([ex.Message]);
        }
    }
}
