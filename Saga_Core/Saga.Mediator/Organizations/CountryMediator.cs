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

namespace Saga.Mediator.Organizations.CountryMediator;

#region "Get List Country"
#region "Query"
    public sealed record GetCountriesQuery(Expression<Func<Country, bool>>[] wheres) : IRequest<CountryList>;
#endregion
#region "Handler"
    public sealed class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, CountryList>
    {
        private readonly IDataContext _context;

        public GetCountriesQueryHandler(IDataContext context)
        {
            _context = context;
        }

    public async Task<CountryList> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Countries.AsQueryable().Where(b => b.DeletedAt == null);

        request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });

        var countries = await queries.ToListAsync();

        var viewModel = new CountryList
        {
            Countries = countries.Select(country => country.ConvertToViewModelCountryListItem())
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Get List Country With Pagination"
#region "Query"
    public sealed record GetCountriesPaginationQuery(PaginationConfig pagination) : IRequest<PaginatedList<Country>>;
#endregion
#region "Handler"
    public sealed class GetCountriesPaginationQueryHandler : IRequestHandler<GetCountriesPaginationQuery, PaginatedList<Country>>
    {
        private readonly IDataContext _context;

        public GetCountriesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Country>> Handle(GetCountriesPaginationQuery request, CancellationToken cancellationToken)
        {

            var queries = _context.Countries.AsQueryable().Where(b => b.DeletedAt == null);
            string search = request.pagination.Find;

            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Description, $"%{search}%"));
            }

            var countries = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            return await Task.FromResult(countries);
        }
    }
#endregion
#endregion

#region "Get By Id Country"
#region "Query"
public sealed record GetCountryQuery(Guid Key) : IRequest<CountryForm>;
#endregion
#region "Handler"
    public sealed class GetCountryQueryHandler : IRequestHandler<GetCountryQuery, CountryForm>
    {
        private readonly IDataContext _context;

        public GetCountryQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<CountryForm> Handle(GetCountryQuery request, CancellationToken cancellationToken)
        {
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Key == request.Key);

            if (country == null || country.DeletedAt != null)
            {
                throw new InvalidOperationException("Country not found or has been deleted.");
            }

            return country.ConvertToViewModelCountryForm();
        }
    }
#endregion
#endregion

#region "Save Country"
#region "Command"
    public sealed record SaveCountryCommand(CountryDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveCountryCommandHandler : IRequestHandler<SaveCountryCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<CountryDto> _validator;

        public SaveCountryCommandHandler(IDataContext context, IValidator<CountryDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveCountryCommand command, CancellationToken cancellationToken)
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

                var country = command.Form.ConvertToEntity();

                if (country.Key == Guid.Empty)
                {
                    country.Key = Guid.NewGuid();
                }

                //Check if country exists
                var existingCountry = await _context.Countries
                                            .FirstOrDefaultAsync(c => c.Key == command.Form.Key && c.DeletedAt == null, cancellationToken);

                if (existingCountry == null)
                {
                    //Add new Country
                    _context.Countries.Add(country);
                }
                else
                {
                    //Update existing Country
                    country.CreatedAt = existingCountry.CreatedAt;
                    country.CreatedBy = existingCountry.CreatedBy;
                    _context.Countries.Entry(existingCountry).CurrentValues.SetValues(country);
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

#region "Delete Country"
#region "Command"
    public sealed record DeleteCountryCommand(Guid Key) : IRequest<Result<Country>>;
#endregion
#region "Handler"
    public sealed class DeleteCountryCommandHandler : IRequestHandler<DeleteCountryCommand, Result<Country>>
    {
        private readonly IDataContext _context;

        public DeleteCountryCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<Country>> Handle(DeleteCountryCommand command, CancellationToken cancellationToken)
        {
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Key == command.Key);

            try
            {
                if (country == null)
                {
                    throw new Exception("Country Not Found");
                }

                _context.Countries.Remove(country);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Country>.Failure(new[] { ex.Message });
            }

            return Result<Country>.Success(country);
        }
    }
#endregion
#endregion
