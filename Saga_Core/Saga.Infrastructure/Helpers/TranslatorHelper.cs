
using Microsoft.AspNetCore.Http;

namespace Saga.Infrastructure.Helpers;

public static class TranslatorHelper
{
    public static string Translate(HttpContext httpContext, string word)
    {
        //var context = (LocalDataContext)httpContext.RequestServices.GetService(typeof(LocalDataContext));
        //try
        //{
        //    var session = httpContext.Session;
        //    ApplicationLanguage find = context.Language.Where(f => f.Word == word).FirstOrDefault()
        //        ?? throw new Exception("Not Found");

        //    string langSession = session.GetString(SessionLogin.Language);
        //    ProfileLanguage profileLanguage = (ProfileLanguage)Enum.Parse(typeof(ProfileLanguage), langSession);

        //    string translate = "";

        //    translate = profileLanguage switch
        //    {
        //        ProfileLanguage.Bahasa => find?.Indonesia ?? string.Empty,
        //        ProfileLanguage.English => find?.English ?? string.Empty,
        //        ProfileLanguage.Korea => find?.Korea ?? string.Empty,
        //        ProfileLanguage.Arabic => find?.Arabic ?? string.Empty,
        //        ProfileLanguage.China => find?.Chinese ?? string.Empty,
        //        _ => find?.English ?? string.Empty,
        //    };

        //    //translate = find?.Korea ?? string.Empty;

        //    return string.IsNullOrEmpty(translate) ? word : translate;

        //}
        //catch (Exception e)
        //{
        //    ApplicationLanguage language = new();
        //    language.Word = word;
        //    language.English = word;

        //    context.Language.Add(language);
        //    context.SaveChanges();
        //}
        return word;
    }
}
