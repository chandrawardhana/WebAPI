
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Systems;
using Saga.Domain.Entities.Systems;
using Saga.DomainShared;
using Saga.Persistence.Context;

namespace Saga.Mediator.Systems.OrganizationAccessMediator;


#region Get List
public record GetOrganizationAccessQuery() : IRequest<IEnumerable<OrganizationAccess>>;
public class GetOrganizationAccessQueryHandler(IDataContext _context) : IRequestHandler<GetOrganizationAccessQuery, IEnumerable<OrganizationAccess>>
{
    public async Task<IEnumerable<OrganizationAccess>> Handle(GetOrganizationAccessQuery request, CancellationToken cancellationToken)
        => await _context.OrganizationAccess
                .OrderBy(x => x.AccessName)
                .ToArrayAsync();
}
#endregion

#region Get By ID
public record GetOrganizationAccessByKeyQuery(Guid Key) : IRequest<OrganizationAccess?>;
public sealed class GetOrganizationAccessByKeyQueryHandler(IDataContext _context) : IRequestHandler<GetOrganizationAccessByKeyQuery, OrganizationAccess?>
{
    public async Task<OrganizationAccess?> Handle(GetOrganizationAccessByKeyQuery request, CancellationToken cancellationToken)
    {
        var find = await _context.OrganizationAccess.FirstOrDefaultAsync(x => x.Key == request.Key);
        return await Task.FromResult(find);
    }
}
#endregion

#region SAVE || UPDATE
public record SaveOrganizationAccessCommand(OrganizationAccessDto dto) : IRequest<Result>;
public sealed class SaveOrganizationAccessCommandHandler(IDataContext _context) : IRequestHandler<SaveOrganizationAccessCommand, Result>
{
    public async Task<Result> Handle(SaveOrganizationAccessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = request.dto;
            if (string.IsNullOrEmpty(dto.AccessName))
                throw new Exception("Access Name's Required.");

            if (dto.Key == Guid.Empty)
                dto.Key = Guid.NewGuid();

            var access = dto.ConvertToEntity();
            var find = _context.OrganizationAccess.FirstOrDefault(x => x.Key == access.Key);
            if (find == null)
            {
                _context.OrganizationAccess.Add(access);
            }
            else
            {
                _context.OrganizationAccess.Entry(find).CurrentValues.SetValues(access);
            }
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

#region Delete
public record DeleteOrganizationAccessCommand(Guid Key) : IRequest<Result>;
public class DeleteOrganizationAccessCommandHandler(IDataContext _context) : IRequestHandler<DeleteOrganizationAccessCommand, Result>
{
    public async Task<Result> Handle(DeleteOrganizationAccessCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var find = await _context.OrganizationAccess.FirstOrDefaultAsync(x => x.Key == command.Key);
            if (find == null)
                throw new Exception("Navigation Access not found.");

            _context.OrganizationAccess.Remove(find);

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