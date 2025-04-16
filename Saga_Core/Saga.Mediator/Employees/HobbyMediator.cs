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

namespace Saga.Mediator.Employees.HobbyMediator;

#region "Get List Hobby"
#region "Query"
    public sealed record GetHobbiesQuery(Expression<Func<Hobby, bool>>[] wheres) : IRequest<HobbyList>;
#endregion
#region "Handler"
    public sealed class GetHobbiesQueryHandler : IRequestHandler<GetHobbiesQuery, HobbyList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetHobbiesQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<HobbyList> Handle(GetHobbiesQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Hobbies.AsQueryable().Where(b => b.DeletedAt == null);

            request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });

            var hobbies = await queries.ToListAsync();

            var viewModel = new HobbyList
            {
                Hobbies = _mapper.Map<IEnumerable<Hobby>>(hobbies)
            };

            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetHobbiesMapper : Profile
    {
        public GetHobbiesMapper()
        {
            CreateMap<Hobby, HobbyList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Hobby With Pagination"
#region "Query"
    public sealed record GetHobbiesPaginationQuery(PaginationConfig pagination) : IRequest<PaginatedList<Hobby>>;
#endregion
#region "Handler"
    public sealed class GetHobbiesPaginationQueryHandler : IRequestHandler<GetHobbiesPaginationQuery, PaginatedList<Hobby>>
    {
        private readonly IDataContext _context;

        public GetHobbiesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Hobby>> Handle(GetHobbiesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Hobbies.AsQueryable().Where(b => b.DeletedAt == null);
            string search = request.pagination.Find;

            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Description, $"%{search}%"));
            }

            var hobbies = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            return await Task.FromResult(hobbies);
        }
    }
#endregion
#endregion

#region "Get By Id Hobby"
#region "Query"
    public sealed record GetHobbyQuery(Guid Key) : IRequest<HobbyForm>;
#endregion
#region "Handler"
    public sealed class GetHobbyQueryHandler : IRequestHandler<GetHobbyQuery, HobbyForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetHobbyQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<HobbyForm> Handle(GetHobbyQuery request, CancellationToken cancellationToken)
        {
            var hobby = await _context.Hobbies.FirstOrDefaultAsync(h => h.Key == request.Key);
            if (hobby == null || hobby.DeletedAt != null)
            {
                throw new InvalidOperationException("Hobby not found or has been deleted.");
            }
            return _mapper.Map<HobbyForm>(hobby);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetHobbyMapper : Profile
    {
        public GetHobbyMapper()
        {
            CreateMap<Hobby, HobbyForm>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Save Hobby"
#region "Command"
    public sealed record SaveHobbyCommand(HobbyDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveHobbyCommandHandler : IRequestHandler<SaveHobbyCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<HobbyDto> _validator;

        public SaveHobbyCommandHandler(IDataContext context, IMapper mapper, IValidator<HobbyDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveHobbyCommand command, CancellationToken cancellationToken)
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
                    //Create Hobby
                    var hobby = _mapper.Map<Hobby>(command.Form);
                    hobby.Key = Guid.NewGuid();

                    _context.Hobbies.Add(hobby);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var hobby = _mapper.Map<Hobby>(command.Form);
                    if (hobby == null)
                    {
                        throw new Exception("Hobby Not Found");
                    }

                    _context.Hobbies.Update(hobby);
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
    public sealed class SaveHobbyMapper : Profile
    {
        public SaveHobbyMapper()
        {
            CreateMap<Hobby, HobbyDto>().ReverseMap();
            CreateMap<Hobby, SaveHobbyCommand>().ReverseMap();  
        }
    }
#endregion
#endregion

#region "Delete Hobby"
#region "Command"
    public sealed record DeleteHobbyCommand(Guid Key) : IRequest<Result<Hobby>>;
#endregion
#region "Handler"
    public sealed class DeleteHobbyCommandHandler : IRequestHandler<DeleteHobbyCommand, Result<Hobby>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeleteHobbyCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Hobby>> Handle(DeleteHobbyCommand command, CancellationToken cancellationToken)
        {
            var hobby = await _context.Hobbies.FirstOrDefaultAsync(h => h.Key == command.Key);

            try
            {
                if (hobby == null)
                {
                    throw new Exception("Hobby Not Found");
                }

                _mapper.Map<Hobby>(command);

                _context.Hobbies.Remove(hobby);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Hobby>.Failure(new[] { ex.Message });
            }
            return Result<Hobby>.Success(hobby);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteHobbyMapper : Profile
    {
        public DeleteHobbyMapper()
        {
            CreateMap<Hobby, DeleteHobbyCommand>().ReverseMap();
        }
    }
#endregion
#endregion

