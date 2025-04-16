using DocumentFormat.OpenXml.Spreadsheet;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Entities.Systems;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels;
using Saga.DomainShared;
using Saga.DomainShared.Enums;
using Saga.DomainShared.Extensions;
using Saga.DomainShared.Interfaces;
using Saga.DomainShared.Models;
using Saga.Infrastructure.Interfaces;
using Saga.Mediator.Employees.EmployeeMediator;
using Saga.Mediator.Organizations.CompanyMediator;
using Saga.Mediator.Organizations.OrganizationMediator;
using Saga.Mediator.Organizations.PositionMediator;
using Saga.Mediator.Organizations.TitleMediator;
using System.Linq.Expressions;

namespace Saga.Infrastructure.Services;

public class ApplicationService(
    Lazy<IUserService> _userService,
    Lazy<IAuditLogger> _auditLogger
    ) : IApplicationService
{
    #region Logger
    public async Task WriteLoggerAsync(LogMode mode, ContentOf content, object obj)
        => await _auditLogger.Value.WriteLoggerAsync(mode, content, obj);

    public async Task<IEnumerable<UserLogger>> ReadLoggerAsync(DateOnly startDate, DateOnly endDate)
        => await _auditLogger.Value.ReadLoggerAsync(startDate, endDate);
    #endregion

    #region UserService
    public async Task<UserProfile?> FindUserProfile(Expression<Func<UserProfile, bool>> where)
        => await _userService.Value.FindUserProfile(where);    

    public async Task<Result> SignInAttemp(string Email, string Password)
        => await _userService.Value.SignInAttemp(Email, Password);

    public async Task SignOut()
        => await _userService.Value.SignOut();

    public async Task<Result> SaveUserProfile(UserProfile userProfile, CancellationToken cancellationToken)
        => await _userService.Value.SaveUserProfile(userProfile, cancellationToken);

    public async Task<Result> RemoveUserProfile(Guid userId, CancellationToken cancellationToken)
        => await _userService.Value.RemoveUserProfile(userId, cancellationToken);

    public async Task<Result> SavePassword(UserProfile userProfile, string password, CancellationToken cancellationToken)
        => await _userService.Value.SavePassword(userProfile, password, cancellationToken);

    public async Task<Result> ResetPassword(Guid userId, CancellationToken cancellationToken)
        => await _userService.Value.ResetPassword(userId, cancellationToken);
    #endregion

}
