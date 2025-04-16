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

namespace Saga.Mediator.Employees.ReligionMediator;


#region "Get List Religion"
#region "Query"
    public sealed record GetReligionsQuery(Expression<Func<Religion, bool>>[] wheres) : IRequest<ReligionList>;
#endregion
#region "Handler"
    public sealed class GetReligionsQueryHandler : IRequestHandler<GetReligionsQuery, ReligionList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetReligionsQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ReligionList> Handle(GetReligionsQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Religions.AsQueryable().Where(b => b.DeletedAt == null);

            request.wheres.ToList()
                          .ForEach(x =>
                          {
                              queries = queries.Where(x);
                          });

            var religions = await queries.ToListAsync();

            var viewModel = new ReligionList
            {
                Religions = _mapper.Map<IEnumerable<Religion>>(religions)
            };

            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetReligionsMapper : Profile
    {
        public GetReligionsMapper()
        {
            CreateMap<Religion, ReligionList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Religion With Pagination"
#region "Query"
    public sealed record GetReligionsPaginationQuery(PaginationConfig pagination) : IRequest<PaginatedList<Religion>>;
#endregion
#region "Handler"
    public sealed class GetReligionsPaginationQueryHandler : IRequestHandler<GetReligionsPaginationQuery, PaginatedList<Religion>>
    {
        private readonly IDataContext _context;

        public GetReligionsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Religion>> Handle(GetReligionsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Religions.AsQueryable().Where(b => b.DeletedAt == null);
            string search = request.pagination.Find;

            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Description, $"%{search}%"));
            }

            var religions = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
        
            return await Task.FromResult(religions);
        }
    }
#endregion
#endregion

#region "Get By Id Religion"
#region "Query"
    public sealed record GetReligionQuery(Guid Key) : IRequest<ReligionForm>;
#endregion
#region "Handler"
    public sealed class GetReligionQueryHandler : IRequestHandler<GetReligionQuery, ReligionForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetReligionQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ReligionForm> Handle(GetReligionQuery request, CancellationToken cancellationToken)
        {
            var religion = await _context.Religions.FirstOrDefaultAsync(r => r.Key == request.Key);
            if (religion == null || religion.DeletedAt != null)
            {
                throw new InvalidOperationException("Religion not found or has been deleted.");
            }
            return _mapper.Map<ReligionForm>(religion);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetReligionMapper : Profile
    {
        public GetReligionMapper()
        {
            CreateMap<Religion, ReligionForm>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Save Religion"
#region "Command"
    public sealed record SaveReligionCommand(ReligionDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveReligionCommandHandler : IRequestHandler<SaveReligionCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<ReligionDto> _validator;

        public SaveReligionCommandHandler(IDataContext context, IMapper mapper, IValidator<ReligionDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveReligionCommand command, CancellationToken cancellationToken)
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
                    //Create Religion
                    var religion = _mapper.Map<Religion>(command.Form);
                    religion.Key = Guid.NewGuid();

                    _context.Religions.Add(religion);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var religion = _mapper.Map<Religion>(command.Form);
                    if (religion == null)
                    {
                        throw new Exception("Religion Not Found");
                    }
                    _context.Religions.Update(religion);
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
    public sealed class SaveReligionMapper : Profile
    {
        public SaveReligionMapper()
        {
            CreateMap<Religion, ReligionDto>().ReverseMap();
            CreateMap<Religion, SaveReligionCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Religion"
#region "Command"
    public sealed record DeleteReligionCommand(Guid Key) : IRequest<Result<Religion>>;
#endregion
#region "Handler"
    public sealed class DeleteReligionCommandHandler : IRequestHandler<DeleteReligionCommand, Result<Religion>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeleteReligionCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Religion>> Handle(DeleteReligionCommand command, CancellationToken cancellationToken)
        {
            var religion = await _context.Religions.FirstOrDefaultAsync(r => r.Key == command.Key);

            try
            {
                if (religion == null)
                {
                    throw new Exception("Religion Not Found");
                }

                _mapper.Map<Religion>(command);
                _context.Religions.Remove(religion);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Religion>.Failure(new[] { ex.Message });
            }
            return Result<Religion>.Success(religion);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteReligionMapper : Profile
    {
        public DeleteReligionMapper()
        {
            CreateMap<Religion, DeleteReligionCommand>().ReverseMap();
        }
    }
#endregion
#endregion

