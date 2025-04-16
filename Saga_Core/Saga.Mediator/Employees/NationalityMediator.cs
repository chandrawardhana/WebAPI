using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Employees;
using Saga.Domain.Entities.Employees;
using Saga.Domain.ViewModels.Employees;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Employees.NationalityMediator;

#region "Get List Nationality"
#region "Query"
public sealed record GetNationalitiesQuery(Expression<Func<Nationality, bool>>[] wheres) : IRequest<IEnumerable<NationalityForm>>;
#endregion
#region "Handler"
public sealed class GetNationalitiesQueryHandler : IRequestHandler<GetNationalitiesQuery, IEnumerable<NationalityForm>>
{
    private readonly IDataContext _context;

    public GetNationalitiesQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NationalityForm>> Handle(GetNationalitiesQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Nationalities.AsQueryable().Where(x => x.DeletedAt == null);
        request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });
        var nationalities = await queries.ToListAsync();

        return nationalities.Select(x => x.ConvertToViewModel());
    }
}
#endregion
#endregion

#region "Get List Nationality With Pagination"
#region "Query"
public sealed record GetNationalitiesPaginationQuery(PaginationConfig pagination) : IRequest<PaginatedList<Nationality>>;
#endregion
#region "Handler"
public sealed class GetNationalitiesPaginationQueryHandler : IRequestHandler<GetNationalitiesPaginationQuery, PaginatedList<Nationality>>
{
    private readonly IDataContext _context;

    public GetNationalitiesPaginationQueryHandler(IDataContext context)
    {
        _context = context;
    }
    public async Task<PaginatedList<Nationality>> Handle(GetNationalitiesPaginationQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Nationalities.AsQueryable().Where(x => x.DeletedAt == null);
        string search = request.pagination.Find;

        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Description, $"%{search}%"));
        }

        var nationalities = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

        return await Task.FromResult(nationalities);
    }
}
#endregion
#endregion

#region "Get By Id Nationality"
#region "Query"
public sealed record GetNationalityQuery(Guid Key) : IRequest<NationalityForm>;
#endregion
#region "Handler"
public sealed class GetNationalityQueryHandler : IRequestHandler<GetNationalityQuery, NationalityForm>
{
    private readonly IDataContext _context;
    public GetNationalityQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<NationalityForm> Handle(GetNationalityQuery request, CancellationToken cancellationToken)
    {
        var nationality = await _context.Nationalities.FirstOrDefaultAsync(x => x.Key == request.Key);
        if (nationality == null || nationality.DeletedAt != null)
        {
            throw new InvalidOperationException("Nationality not found or has been deleted.");
        }

        return nationality.ConvertToViewModel();
    }
}
#endregion
#endregion

#region "Save Nationality"
#region "Command"
public sealed record SaveNationalityCommand(NationalityDto Form) : IRequest<Result>;
#endregion
#region "Handler"
public sealed class SaveNationalityCommandHandler : IRequestHandler<SaveNationalityCommand, Result>
{
    private readonly IDataContext _context;
    private readonly IValidator<NationalityDto> _validator;

    public SaveNationalityCommandHandler(IDataContext context, IValidator<NationalityDto> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<Result> Handle(SaveNationalityCommand command, CancellationToken cancellationToken)
    {
        try
        {

            ValidationResult validator = await _validator.ValidateAsync(command.Form);
            if (!validator.IsValid)
            {
                var failures = validator.Errors
                                        .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                        .ToList();
                return Result.Failure(failures);
            }

            var nationality = command.Form.ConvertToEntity();

            if (nationality.Key == Guid.Empty)
            {
                nationality.Key = Guid.NewGuid();
            }

            //Check if nationality exist
            var existingNationality = await _context.Nationalities.FirstOrDefaultAsync(x => x.Key == nationality.Key, cancellationToken);

            if (existingNationality == null)
            {
                //Add new Nationality
                _context.Nationalities.Add(nationality);
            }
            else
            {
                //Update existing Nationality
                nationality.CreatedAt = existingNationality.CreatedAt;
                nationality.CreatedBy = existingNationality.CreatedBy;
                _context.Nationalities.Entry(existingNationality).CurrentValues.SetValues(nationality);
            }

            var result = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(new[] { ex.Message });
        }
        return Result.Success();
    }
}
#endregion
#endregion

#region "Delete Nationality"
#region "Command"
public sealed record DeleteNationalityCommand(Guid Key) : IRequest<Result<Nationality>>;
#endregion
#region "Handler"
public sealed class DeleteNationalityCommandHandler : IRequestHandler<DeleteNationalityCommand, Result<Nationality>>
{
    private readonly IDataContext _context;

    public DeleteNationalityCommandHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<Result<Nationality>> Handle(DeleteNationalityCommand command, CancellationToken cancellationToken)
    {
        var nationalty = await _context.Nationalities.FirstOrDefaultAsync(x => x.Key == command.Key, cancellationToken);

        try
        {
            if (nationalty == null)
            {
                throw new Exception("Nationality Not Found");
            }

            _context.Nationalities.Remove(nationalty);
            var result = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<Nationality>.Failure(new[] { ex.Message });
        }

        return Result<Nationality>.Success(nationalty);
    }
}
#endregion
#endregion
