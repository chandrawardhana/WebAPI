using AutoMapper;
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

namespace Saga.Mediator.Organizations.CurrencyMediator;

#region "Get List Currency"
#region "Query"
    public sealed record GetCurrenciesQuery(Expression<Func<Currency, bool>>[] wheres) : IRequest<CurrencyList>;
#endregion
#region "Handler"
    public sealed class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQuery, CurrencyList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetCurrenciesQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CurrencyList> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Currencies.AsQueryable().Where(b => b.DeletedAt == null);
            request.wheres.ToList()
                                 .ForEach(x =>
                                 {
                                     queries = queries.Where(x);
                                 });
            var currencies = await queries.ToListAsync();
            var viewModel = new CurrencyList
            {
                Currencies = _mapper.Map<IEnumerable<Currency>>(currencies)
            };
            throw new NotImplementedException();
        }
    }
#endregion
#region "Mapper"
    public sealed class GetCurrenciesMapper : Profile
    {
        public GetCurrenciesMapper()
        {
            CreateMap<Currency, CurrencyList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Currency With Pagination"
#region "Query"
    public sealed record GetCurrenciesPaginationQuery(Expression<Func<Currency, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<Currency>>;
#endregion
#region "Handler"
    public sealed class GetCurrenciesPaginationQueryHandler : IRequestHandler<GetCurrenciesPaginationQuery, PaginatedList<Currency>>
    {
        private readonly IDataContext _context;

        public GetCurrenciesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }
        public async Task<PaginatedList<Currency>> Handle(GetCurrenciesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Currencies.AsQueryable().Where(b => b.DeletedAt == null);
            string search = request.pagination.Find;

            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(p => EF.Functions.ILike(p.Code, $"%{search}%") || EF.Functions.ILike(p.Name, $"%{search}%") || EF.Functions.ILike(p.Description, $"%{search}%") || EF.Functions.ILike(p.Symbol, $"%{search}%"));
            }

            request.wheres.ToList()
                                 .ForEach(x =>
                                 {
                                     queries = queries.Where(x);
                                 });
            var currencies = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
            return await Task.FromResult(currencies);
        }
    }
#endregion
#endregion

#region "Get By Id Currency"
#region "Query"
    public sealed record GetCurrencyQuery(Guid Key) : IRequest<CurrencyForm>;
#endregion
#region "Handler"
    public sealed class GetCurrencyQueryHandler : IRequestHandler<GetCurrencyQuery, CurrencyForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetCurrencyQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CurrencyForm> Handle(GetCurrencyQuery request, CancellationToken cancellationToken)
        {
            var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Key == request.Key && c.DeletedAt == null);
            if (currency == null)
                throw new InvalidOperationException("Currency not found or has been deleted.");
            return _mapper.Map<CurrencyForm>(currency); 
        }
    }
#endregion
#region "Mapper"
    public sealed class GetCurrencyMapper : Profile
    {
        public GetCurrencyMapper()
        {
            CreateMap<Currency, CurrencyForm>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Save Currency"
#region "Command"
    public sealed record SaveCurrencyCommand(CurrencyDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveCurrencyCommandHandler : IRequestHandler<SaveCurrencyCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<CurrencyDto> _validator;

        public SaveCurrencyCommandHandler(IDataContext context, IMapper mapper, IValidator<CurrencyDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveCurrencyCommand command, CancellationToken cancellationToken)
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

                if (command.Form.Key == Guid.Empty || command.Form.Key == null)
                {
                    //Create Currency
                    var currency = _mapper.Map<Currency>(command.Form);
                    currency.Key = Guid.NewGuid();

                    _context.Currencies.Add(currency);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                } 
                else
                {
                    var currency = _mapper.Map<Currency>(command.Form);
                    if (currency == null)
                        throw new InvalidOperationException("Currency not found.");

                    _context.Currencies.Update(currency);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }
            return Result.Success();
        }
    }
#endregion
#region "Mapper"
    public sealed class SaveCurrencyMapper : Profile
    {
        public SaveCurrencyMapper()
        {
            CreateMap<Currency, CurrencyDto>().ReverseMap();
            CreateMap<Currency, SaveCurrencyCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Currency"
#region "Command"
    public sealed record DeleteCurrencyCommand(Guid Key) : IRequest<Result<Currency>>;
#endregion
#region "Handler"
    public sealed class DeleteCurrencyCommandHandler : IRequestHandler<DeleteCurrencyCommand, Result<Currency>>
    {
        private readonly IDataContext _context;

        public DeleteCurrencyCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<Currency>> Handle(DeleteCurrencyCommand command, CancellationToken cancellationToken)
        {
            var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Key == command.Key);

            try
            {
                if (currency == null)
                    throw new InvalidOperationException("Currency not found.");

                _context.Currencies.Remove(currency);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Currency>.Failure(new[] { ex.Message });
            }
            return Result<Currency>.Success(currency);
        }
    }
#endregion
#endregion
