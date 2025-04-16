
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Systems;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Systems;
using Saga.DomainShared;
using Saga.Mediator.Attendances.ShiftMediator;
using Saga.Mediator.Systems.AssetMediator;
using Saga.Persistence.Context;

namespace Saga.Mediator.Systems.NavigationAccessMediator;

#region Get List
public record GetNavigationAccessQuery() : IRequest<IEnumerable<NavigationAccess>>;
public sealed class GetNavigationAccessQueryHandler(IDataContext _context) : IRequestHandler<GetNavigationAccessQuery, IEnumerable<NavigationAccess>>
{
    public async Task<IEnumerable<NavigationAccess>> Handle(GetNavigationAccessQuery request, CancellationToken cancellationToken)
        => await _context.NavigationAccess.OrderBy(x => x.AccessName).ToArrayAsync();
}
#endregion

#region Get By ID
public record GetNavigationAccessByKeyQuery(Guid Key) : IRequest<NavigationAccess?>;
public sealed class GetNavigationAccessByKeyQueryHandler(IDataContext _context) : IRequestHandler<GetNavigationAccessByKeyQuery, NavigationAccess?>
{
    public async Task<NavigationAccess?> Handle(GetNavigationAccessByKeyQuery request, CancellationToken cancellationToken)
    {
        var find = await _context.NavigationAccess.FirstOrDefaultAsync(x => x.Key == request.Key);
        if(find != null)
        {
            find.AccessDetails = await _context.NavigationAccessDetail
                                        .Where(x => x.NavigationAccessKey == find.Key)
                                        .ToArrayAsync();
        }
        return await Task.FromResult(find);
    }
}
#endregion

#region SAVE || UPDATE
public record SaveNavigationAccessCommand(NavigationAccessDto dto) : IRequest<Result>;
public sealed class SaveNavigationAccessCommandHandler(IDataContext _context) : IRequestHandler<SaveNavigationAccessCommand, Result>
{
    public async Task<Result> Handle(SaveNavigationAccessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.dto;
            if (string.IsNullOrEmpty(dto.AccessName))
                throw new Exception("Access Name's Required.");

            if(dto.Key == Guid.Empty)
                dto.Key = Guid.NewGuid();

            var access = dto.ConvertToEntity();
            var find = _context.NavigationAccess.FirstOrDefault(x => x.Key == access.Key);
            if(find == null)
            {
                _context.NavigationAccess.Add(access);
            }
            else
            {
                _context.NavigationAccess.Entry(find).CurrentValues.SetValues(access);
                _context.NavigationAccessDetail
                        .Where(x => x.NavigationAccessKey == find.Key)
                        .ExecuteDelete();
            }
            _context.NavigationAccessDetail.AddRange(access.AccessDetails);
            await _context.SaveChangesAsync(cancellationToken);

            return await Task.FromResult(Result.Success());
        }catch (Exception ex)
        {
            return await Task.FromResult(Result.Failure([ex.Message]));
        }
    }
}
#endregion

#region Delete
public record DeleteNavigationAccessCommand(Guid Key) : IRequest<Result>;
public class DeleteNavigationAccessCommandHandler(IDataContext _context) : IRequestHandler<DeleteNavigationAccessCommand, Result>
{
    public async Task<Result> Handle(DeleteNavigationAccessCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var find = await _context.NavigationAccess.FirstOrDefaultAsync(x => x.Key == command.Key);
            if (find == null)
                throw new Exception("Navigation Access not found.");

            _context.NavigationAccessDetail.Where(x => x.NavigationAccessKey == find.Key).ExecuteDelete();
            _context.NavigationAccess.Remove(find);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure([ex.Message]);
        }
    }
}
#endregion