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

namespace Saga.Mediator.Organizations.PositionMediator;

#region "Get List Position"
#region "Query"
    public sealed record GetPositionsQuery(Expression<Func<Position, bool>>[] wheres) : IRequest<PositionList>;
#endregion
#region "Handler"
    public sealed class GetPositionsQueryHandler : IRequestHandler<GetPositionsQuery, PositionList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetPositionsQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PositionList> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Positions.AsQueryable().Where(b => b.DeletedAt == null);

            request.wheres.ToList()
                          .ForEach(x =>
                          {
                              queries = queries.Where(x);
                          });

            var positions = await queries.ToListAsync();

            var viewModel = new PositionList
            {
                Positions = _mapper.Map<IEnumerable<Position>>(positions)
            };

            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetPositionsMapper : Profile
    {
        public GetPositionsMapper()
        {
            CreateMap<Position, PositionList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Position With Pagination"
#region "Query"
    public sealed record GetPositionsPaginationQuery(Expression<Func<Position, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<Position>>;
#endregion
#region "Handler"
    public sealed class GetPositionsPaginationQueryHandler : IRequestHandler<GetPositionsPaginationQuery, PaginatedList<Position>>
    {
        private readonly IDataContext _context;

        public GetPositionsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Position>> Handle(GetPositionsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Positions.AsQueryable().Where(b => b.DeletedAt == null);
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

            var positions = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
            positions.Items.ForEach(x =>
            {
                x.Company = _context.Companies.FirstOrDefault(f => f.Key == x.CompanyKey);
            });

            return await Task.FromResult(positions);
        }
    }
#endregion
#endregion

#region "Get By Id Position"
#region "Query"
    public sealed record GetPositionQuery(Guid Key) : IRequest<PositionForm>;
#endregion
#region "Handler"
    public sealed class GetPositionQueryHandler : IRequestHandler<GetPositionQuery, PositionForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetPositionQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PositionForm> Handle(GetPositionQuery request, CancellationToken cancellationToken)
        {
            var position = await _context.Positions.FirstOrDefaultAsync(p => p.Key == request.Key);
            if (position == null)
            {
                throw new InvalidOperationException("Position not found or has been deleted.");
            }

            var company = (position.CompanyKey != Guid.Empty)
                                ? await _context.Companies.FirstOrDefaultAsync(c => c.Key == position.CompanyKey) : null;
            position.Company = company;

            return _mapper.Map<PositionForm>(position);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetPositionMapper : Profile
    {
        public GetPositionMapper()
        {
            CreateMap<Position, PositionForm>().ReverseMap()
                    .ForMember(dest => dest.CompanyKey, opt => opt.MapFrom(src => src.CompanyKey))
                    .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company));
        }
    }
#endregion
#endregion

#region "Save Position"
#region "Command"
    public sealed record SavePositionCommand(PositionDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SavePositionCommandHandler : IRequestHandler<SavePositionCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<PositionDto> _validator;

        public SavePositionCommandHandler(IDataContext context, IMapper mapper, IValidator<PositionDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result> Handle(SavePositionCommand command, CancellationToken cancellationToken)
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
                    //Create Position
                    var position = _mapper.Map<Position>(command.Form);
                    position.Key = Guid.NewGuid();

                    _context.Positions.Add(position);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var position = _mapper.Map<Position>(command.Form);
                    if (position == null)
                    {
                        throw new InvalidOperationException("Position not found.");
                    }
                    _context.Positions.Update(position);
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
    public sealed class SavePositionMapper : Profile
    {
        public SavePositionMapper()
        {
            CreateMap<Position, PositionDto>().ReverseMap();
            CreateMap<Position, SavePositionCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Position"
#region "Command"
    public sealed record DeletePositionCommand(Guid Key) : IRequest<Result<Position>>;
#endregion
#region "Handler"
    public sealed class DeletePositionCommandHandler : IRequestHandler<DeletePositionCommand, Result<Position>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeletePositionCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Position>> Handle(DeletePositionCommand command, CancellationToken cancellationToken)
        {
            var position = await _context.Positions.FirstOrDefaultAsync(p => p.Key == command.Key);

            try
            {
                if (position == null)
                {
                    throw new Exception("Position not found");
                }

                _mapper.Map<Position>(position);

                _context.Positions.Remove(position);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Position>.Failure(new[] { ex.Message });
            }
            return Result<Position>.Success(position);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeletePositionMapper : Profile
    {
        public DeletePositionMapper()
        {
            CreateMap<Position, DeletePositionCommand>().ReverseMap();
        }
    }
#endregion
#endregion
