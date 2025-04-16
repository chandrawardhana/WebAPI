using AutoMapper;
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

namespace Saga.Mediator.Employees.EthnicMediator;

#region "Get List Ethnic"
#region "Query"
    public sealed record GetEthnicsQuery(Expression<Func<Ethnic, bool>>[] wheres) : IRequest<EthnicList>;
#endregion
#region "Handler"
    public sealed class GetEthnicsQueryHandler : IRequestHandler<GetEthnicsQuery, EthnicList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetEthnicsQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EthnicList> Handle(GetEthnicsQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Ethnics.AsQueryable().Where(b => b.DeletedAt == null);
            request.wheres.ToList()
                          .ForEach(x =>
                          {
                              queries = queries.Where(x);
                          });
            var ethnics = await queries.ToListAsync();
            var viewModel = new EthnicList
            {
                Ethnics = _mapper.Map<IEnumerable<Ethnic>>(ethnics)
            };
            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetEthnicsMapper : Profile
    {
        public GetEthnicsMapper()
        {
            CreateMap<Ethnic, EthnicList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Ethnic With Pagination"
#region "Query"
public sealed record GetEthnicsPaginationQuery(PaginationConfig pagination) : IRequest<PaginatedList<Ethnic>>;
#endregion
#region "Handler"
public sealed class GetEthnicsPaginationQueryHandler : IRequestHandler<GetEthnicsPaginationQuery, PaginatedList<Ethnic>>
{
    private readonly IDataContext _context;

    public GetEthnicsPaginationQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<Ethnic>> Handle(GetEthnicsPaginationQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Ethnics.AsQueryable().Where(b => b.DeletedAt == null);
        string search = request.pagination.Find;

        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(p => EF.Functions.ILike(p.Code, $"%{search}%") || EF.Functions.ILike(p.Name, $"%{search}%") || EF.Functions.ILike(p.Description, $"%{search}%"));
        }

        var ethnics = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
        return await Task.FromResult(ethnics);
    }
}
#endregion
#endregion

#region "Get By Id Ethnic"
#region "Query"
    public sealed record GetEthnicQuery(Guid Key) : IRequest<EthnicForm>;
#endregion
#region "Handler"
    public sealed class GetEthnicQueryHandler : IRequestHandler<GetEthnicQuery, EthnicForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetEthnicQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EthnicForm> Handle(GetEthnicQuery request, CancellationToken cancellationToken)
        {
            var ethnic = await _context.Ethnics.FirstOrDefaultAsync(b => b.Key == request.Key);
            if (ethnic == null || ethnic.DeletedAt != null)
            {
                throw new InvalidOperationException("Ethnic not found or has been deleted.");
            }
            return _mapper.Map<EthnicForm>(ethnic);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetEthnicMapper : Profile
    {
        public GetEthnicMapper()
        {
            CreateMap<Ethnic, EthnicForm>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Save Ethnic"
#region "Command"
    public sealed record SaveEthnicCommand(EthnicDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveEthnicHandler : IRequestHandler<SaveEthnicCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<EthnicDto> _validator;

        public SaveEthnicHandler(IDataContext context, IMapper mapper, IValidator<EthnicDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveEthnicCommand command, CancellationToken cancellationToken)
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
                    //Create Ethnic
                    var ethnic = _mapper.Map<Ethnic>(command.Form);
                    ethnic.Key = Guid.NewGuid();

                    _context.Ethnics.Add(ethnic);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var ethnic = _mapper.Map<Ethnic>(command.Form);
                    if (ethnic == null)
                    {
                        throw new Exception("Ethnic Not Found");
                    }
                    _context.Ethnics.Update(ethnic);
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
    public sealed class SaveEthnicMapper : Profile
    {
        public SaveEthnicMapper()
        {
            CreateMap<Ethnic, EthnicDto>().ReverseMap();
            CreateMap<Ethnic, SaveEthnicCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Ethnic"
#region "Command"
    public sealed record DeleteEthnicCommand(Guid Key) : IRequest<Result<Ethnic>>;
#endregion
#region "Handler"
    public sealed class DeleteEthnicCommandHandler : IRequestHandler<DeleteEthnicCommand, Result<Ethnic>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeleteEthnicCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Ethnic>> Handle(DeleteEthnicCommand command, CancellationToken cancellationToken)
        {
            var ethnic = await _context.Ethnics.FirstOrDefaultAsync(b => b.Key == command.Key);

            try
            {
                if (ethnic == null)
                {
                    throw new InvalidOperationException("Ethnic not found.");
                }
                _mapper.Map<Ethnic>(command);
                _context.Ethnics.Remove(ethnic);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Ethnic>.Failure(new[] { ex.Message });
            }
            return Result<Ethnic>.Success(ethnic);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteEthnicMapper : Profile
    {
        public DeleteEthnicMapper()
        {
            CreateMap<Ethnic, DeleteEthnicCommand>().ReverseMap();
        }
    }
#endregion
#endregion
