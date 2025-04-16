
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Systems;
using Saga.Domain.Entities.Systems;
using Saga.DomainShared;
using Saga.Mediator.Systems.NavigationAccessMediator;
using Saga.Persistence.Context;
using System.Linq.Expressions;

namespace Saga.Mediator.Systems.UserManagementMediator;

#region GET LIST
public record GetUserManagementsQuery(Expression<Func<UserProfile, bool>>[] wheres) : IRequest<IEnumerable<UserProfile>>;
public class GetUserManagementsQueryHandler(
    IDataContext _context
) : IRequestHandler<GetUserManagementsQuery, IEnumerable<UserProfile>>
{
    public async Task<IEnumerable<UserProfile>> Handle(GetUserManagementsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserProfile.AsQueryable();
        foreach(var where in request.wheres)
        {
            query = query.Where(where);
        }
        return await query.ToArrayAsync();
    }
}
#endregion

#region GET BY ID
public record GetUserManagementQuery(Expression<Func<UserProfile, bool>> where) : IRequest<UserProfile?>;
public class GetUserManagementQueryHandler(
    IDataContext _context
) : IRequestHandler<GetUserManagementQuery, UserProfile?>
{
    public async Task<UserProfile?> Handle(GetUserManagementQuery request, CancellationToken cancellationToken)
    {
        var find = _context.UserProfile.FirstOrDefault(request.where);
        if(find != null)
        {
            find.Employee = _context.Employees.FirstOrDefault(x => x.Key == find.EmployeeKey);
            find.NavigationAccess = _context.NavigationAccess.FirstOrDefault(x => x.Key == find.NavigationAccessKey);
            find.OrganizationAccess = _context.OrganizationAccess.FirstOrDefault(x => x.Key == find.OrganizationAccessKey);
        }
        return await Task.FromResult(find);
    }
}
#endregion

#region DELETE
public record DeleteUserManagementCommand(Expression<Func<UserProfile, bool>> where) : IRequest<Result>;
public class DeleteUserManagementCommandHandler(
    IDataContext _context
) : IRequestHandler<DeleteUserManagementCommand, Result>
{
    public async Task<Result> Handle(DeleteUserManagementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var find = _context.UserProfile.FirstOrDefault(request.where)
                ?? throw new Exception("User's Not Found");

            _context.UserProfile.Remove(find);
            await _context.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return await Task.FromResult(Result.Failure([ex.Message]));
        }
    }
}
#endregion