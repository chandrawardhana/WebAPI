using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Saga.Domain.Entities.Systems;
using Saga.Domain.Enums;
using Saga.DomainShared;
using Saga.DomainShared.Constants;
using Saga.DomainShared.Helpers;
using Saga.DomainShared.Interfaces;
using Saga.Persistence.Context;

namespace Saga.Infrastructure.Services;

public class CurrentUserService(
    IHttpContextAccessor _httpContextAccessor
) : ICurrentUser
{
    public string UserId => _httpContextAccessor.HttpContext?.Session.GetString(SessionLogin.UserId) ?? string.Empty;
    public string Email => _httpContextAccessor.HttpContext?.Session.GetString(SessionLogin.Email) ?? string.Empty;
    public ProfileLanguage Language => GetLanguage();
     
    private ProfileLanguage GetLanguage()
    {
        var lang = _httpContextAccessor.HttpContext?.Session.GetString(SessionLogin.Language) ?? string.Empty;
        if(string.IsNullOrEmpty(lang))
            return ProfileLanguage.English;

        return Enum.Parse<ProfileLanguage>(lang);
    }
}
