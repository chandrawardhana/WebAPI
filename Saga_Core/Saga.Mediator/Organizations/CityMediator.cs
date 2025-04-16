using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Organizations;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.ViewModels.Organizations;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Organizations.CityMediator;

#region "Get List City"
#region "Query"
public sealed record GetCitiesQuery(Expression<Func<City, bool>>[] wheres) : IRequest<CityList>;
#endregion
#region "Handler"
public sealed class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, CityList>
{
    private readonly IDataContext _context;

    public GetCitiesQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<CityList> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Cities.AsQueryable().Where(b => b.DeletedAt == null);

        request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });

        var cities = await queries.ToListAsync();

        var viewModel = new CityList
        {
            Cities = cities.Select(city => city.ConvertToViewModelCityListItem())
        };
        return viewModel;
    }
}
#endregion
#endregion

#region "Get List City With Pagination"
#region "Query"
public sealed record GetCitiesPaginationQuery(Expression<Func<City, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<City>>;
#endregion
#region "Handler"
public sealed class GetCitiesPaginationQueryHandler : IRequestHandler<GetCitiesPaginationQuery, PaginatedList<City>>
{
    private readonly IDataContext _context;

    public GetCitiesPaginationQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<City>> Handle(GetCitiesPaginationQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Cities.AsQueryable().Where(b => b.DeletedAt == null);
        string search = request.pagination.Find;

        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(p => EF.Functions.ILike(p.Code, $"%{search}%") || EF.Functions.ILike(p.Name, $"%{search}%") || EF.Functions.ILike(p.Description, $"%{search}%"));
        }

        request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });
        var cities = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

        cities.Items.ForEach(x =>
        {
            x.Province = _context.Provinces.FirstOrDefault(f => f.Key == x.ProvinceKey);
            x.Country = _context.Countries.FirstOrDefault(f => f.Key == x.CountryKey);
        }); 

        return await Task.FromResult(cities);
    }
}
#endregion
#endregion

#region "Get By Id City"
#region "Query"
public sealed record GetCityQuery(Guid Key) : IRequest<CityForm>;
#endregion
#region "Handler"
public sealed class GetCityQueryHandler : IRequestHandler<GetCityQuery, CityForm>
{
    private readonly IDataContext _context;

    public GetCityQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<CityForm> Handle(GetCityQuery request, CancellationToken cancellationToken)
    {
        var city = await _context.Cities.FirstOrDefaultAsync(c => c.Key == request.Key);
        if (city == null)
        {
            throw new InvalidOperationException("City not found or has been deleted.");
        }

        var province = (city.ProvinceKey != Guid.Empty)
                            ? await _context.Provinces.FirstOrDefaultAsync(c => c.Key == city.ProvinceKey) : null;

        var country = (city.CountryKey != Guid.Empty)
                            ? await _context.Countries.FirstOrDefaultAsync(c => c.Key == city.CountryKey) : null;

        city.Province = province;
        city.Country = country;

        return city.ConvertToViewModelCityForm();
    }
}
#endregion
#endregion

#region "Save City"
#region "Command"
public sealed record SaveCityCommand(CityDto Form) : IRequest<Result>;
#endregion
#region "Handler"
public sealed class SaveCityCommandHandler : IRequestHandler<SaveCityCommand, Result>
{
    private readonly IDataContext _context;
    private readonly IValidator<CityDto> _validator;

    public SaveCityCommandHandler(IDataContext context, IValidator<CityDto> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<Result> Handle(SaveCityCommand command, CancellationToken cancellationToken)
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

            var city = command.Form.ConvertToEntity();

            if (city.Key == Guid.Empty)
            {
                city.Key = Guid.NewGuid();
            }

            //Check if city exists
            var existingCity = await _context.Cities
                                             .FirstOrDefaultAsync(c => c.Key == command.Form.Key && c.DeletedAt == null, cancellationToken);

            if (existingCity == null)
            {
                //Add new City
                _context.Cities.Add(city);
            } 
            else
            {
                //Update existing City
                city.CreatedAt = existingCity.CreatedAt;
                city.CreatedBy = existingCity.CreatedBy;
                _context.Cities.Entry(existingCity).CurrentValues.SetValues(city);
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

#region "Delete City"
#region "Command"
public sealed record DeleteCityCommand(Guid Key) : IRequest<Result<City>>;
#endregion
#region "Handler"
public sealed class DeleteCityHandler : IRequestHandler<DeleteCityCommand, Result<City>>
{
    private readonly IDataContext _context;

    public DeleteCityHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<Result<City>> Handle(DeleteCityCommand command, CancellationToken cancellationToken)
    {
        var city = await _context.Cities.FirstOrDefaultAsync(c => c.Key == command.Key);

        try
        {
            if (city == null)
            {
                throw new Exception("City Not Found");
            }

            _context.Cities.Remove(city);
            var result = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<City>.Failure(new[] { ex.Message });
        }
        return Result<City>.Success(city);
    }
}
#endregion
#endregion
