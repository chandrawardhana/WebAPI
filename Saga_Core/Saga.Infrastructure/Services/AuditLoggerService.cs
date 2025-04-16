
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Saga.DomainShared.Enums;
using Saga.DomainShared.Interfaces;
using Saga.DomainShared.Models;
using Saga.Infrastructure.Constants;
using Saga.Infrastructure.Interfaces;
using Saga.Persistence.Context;
using SQLitePCL;
using System.Data;

namespace Saga.Infrastructure.Services;

public class AuditLoggerService(
    ICurrentUser _currentUser,
    Lazy<IUserService> _userService,
    IConfiguration _configuration,
    LocalDataContext _context) : IAuditLogger
{
    public async Task<IEnumerable<UserLogger>> ReadLoggerAsync(DateOnly startDate, DateOnly endDate)
    {
        List<UserLogger> logs = (await ReadDatabase(startDate, endDate)).ToList();

        return await Task.FromResult(logs.OrderByDescending(x => x.Logdate));
    }

    public async Task WriteLoggerAsync(LogMode mode, ContentOf content, object obj)
        => await WriteDatabase(mode, content, obj);

    private async Task WriteDatabase(LogMode mode, ContentOf content, object obj)
        => await Task.CompletedTask;
    //{
    //    _ = Guid.TryParse(_currentUser.UserId, out Guid userId);

    //    if (userId != Guid.Empty)
    //    {
    //        //var user = await _userService.Value.FindUserProfile(x => x.UserId == userId);

    //        //// parsing object
    //        //Type objType = obj.GetType();
    //        //string codeProperty;
    //        //if (objType.GetProperty("Code") != null)
    //        //    codeProperty = objType.GetProperty("Code").GetValue(obj, null)?.ToString() ?? string.Empty;
    //        //else
    //        //    codeProperty = obj.ToString() ?? string.Empty;

    //        //UserLogger logger = new()
    //        //{
    //        //    LogId = Guid.NewGuid(),
    //        //    CompanyKey = user.Employee?.CompanyKey ?? Guid.Empty,
    //        //    Logdate = DateTime.Now,
    //        //    UserName = user.Employee?.FullName ?? string.Empty,
    //        //    Content = content.ToString(),
    //        //    LogMode = mode,
    //        //    Code = codeProperty
    //        //};

    //        //_context.UserLogger.Add(logger);
    //        //await _context.SaveChangesAsync();
    //        await Task.CompletedTask;
    //    }
    //}
    private async Task<IEnumerable<UserLogger>> ReadDatabase(DateOnly startDate, DateOnly endDate)
    {
        //var loggers = _context.UserLogger
        //                .Where(x => DateOnly.FromDateTime(x.Logdate) >= startDate 
        //                    && DateOnly.FromDateTime(x.Logdate) <= endDate)
        //                .ToArray();
        //return await Task.FromResult(loggers);
        return await Task.FromResult(Enumerable.Empty<UserLogger>());
    }
}
