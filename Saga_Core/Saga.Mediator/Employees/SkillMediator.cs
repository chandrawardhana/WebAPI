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

namespace Saga.Mediator.Employees.SkillMediator;

#region "Get List Skill"
#region "Query"
    public sealed record GetSkillsQuery(Expression<Func<Skill, bool>>[] wheres) : IRequest<SkillList>;
#endregion
#region "Handler"
    public sealed class GetSkillsQueryHandler : IRequestHandler<GetSkillsQuery, SkillList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetSkillsQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<SkillList> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Skills.AsQueryable().Where(b => b.DeletedAt == null);
            request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });
            var skills = await queries.ToListAsync();
            var viewModel = new SkillList
            {
                Skills = _mapper.Map<IEnumerable<Skill>>(skills)
            };
            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetSkillsMapper : Profile
    {
        public GetSkillsMapper()
        {
            CreateMap<Skill, SkillList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Skill With Pagination"
#region "Query"
    public sealed record GetSkillsPaginationQuery(PaginationConfig pagination) : IRequest<PaginatedList<Skill>>;
#endregion
#region "Handler"
    public sealed class GetSkillsPaginationQueryHandler : IRequestHandler<GetSkillsPaginationQuery, PaginatedList<Skill>>
    {
        private readonly IDataContext _context;

        public GetSkillsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Skill>> Handle(GetSkillsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Skills.AsQueryable().Where(b => b.DeletedAt == null);
            string search = request.pagination.Find;

            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(p => EF.Functions.ILike(p.Code, $"%{search}%") || EF.Functions.ILike(p.Name, $"%{search}%") || EF.Functions.ILike(p.Description, $"%{search}%"));
            }
            var skills = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            return await Task.FromResult(skills);
        }
    }
#endregion
#endregion

#region "Get By Id Skill"
#region "Query"
    public sealed record GetSkillQuery(Guid Key) : IRequest<SkillForm>;
#endregion
#region "Handler"
    public sealed class GetSkillQueryHandler : IRequestHandler<GetSkillQuery, SkillForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetSkillQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<SkillForm> Handle(GetSkillQuery request, CancellationToken cancellationToken)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(b => b.Key == request.Key && b.DeletedAt == null);
            if (skill == null || skill.DeletedAt != null)
            {
                throw new InvalidOperationException("Skill not found or has been deleted.");
            }
            return _mapper.Map<SkillForm>(skill);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetSkillMapper : Profile
    {
        public GetSkillMapper()
        {
            CreateMap<Skill, SkillForm>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Save Skill"
#region "Command"
    public sealed record SaveSkillCommand(SkillDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveSkillHandler : IRequestHandler<SaveSkillCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<SkillDto> _validator;

        public SaveSkillHandler(IDataContext context, IMapper mapper, IValidator<SkillDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveSkillCommand command, CancellationToken cancellationToken)
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
                    //Create Skill
                    var skill = _mapper.Map<Skill>(command.Form);
                    skill.Key = Guid.NewGuid();

                    _context.Skills.Add(skill);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var skill = _mapper.Map<Skill>(command.Form);
                    if (skill == null)
                    {
                        throw new Exception("Skill Not Found");
                    }
                    _context.Skills.Update(skill);
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
    public sealed class SaveSkillMapper : Profile
    {
        public SaveSkillMapper()
        {
            CreateMap<Skill, SkillDto>().ReverseMap();
            CreateMap<Skill, SaveSkillCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Skill"
#region "Command"
    public sealed record DeleteSkillCommand(Guid Key) : IRequest<Result<Skill>>;
#endregion
#region "Handler"
    public sealed class DeleteSkillCommandHandler : IRequestHandler<DeleteSkillCommand, Result<Skill>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeleteSkillCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Skill>> Handle(DeleteSkillCommand command, CancellationToken cancellationToken)
        {
            var skill = await _context.Skills.FirstOrDefaultAsync(b => b.Key == command.Key);

            try
            {
                if (skill == null)
                {
                    throw new Exception("Skill not found");
                }
                _mapper.Map<Skill>(command);
                _context.Skills.Remove(skill);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Skill>.Failure(new[] { ex.Message });
            }
            return Result<Skill>.Success(skill);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteSkillMaoper : Profile
    {
        public DeleteSkillMaoper()
        {
            CreateMap<Skill, DeleteSkillCommand>().ReverseMap();
        }
    }
#endregion
#endregion



