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

namespace Saga.Mediator.Organizations.GradeMediator;

#region "Get List Grade"
#region "Query"
    public sealed record GetGradesQuery(Expression<Func<Grade, bool>>[] wheres) : IRequest<GradeList>;
#endregion
#region "Handler"
    public sealed class GetGradesQueryHandler : IRequestHandler<GetGradesQuery, GradeList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetGradesQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<GradeList> Handle(GetGradesQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Grades.AsQueryable().Where(b => b.DeletedAt == null);
            request.wheres.ToList()
                             .ForEach(x =>
                             {
                                 queries = queries.Where(x);
                             });

            var grades = await queries.ToListAsync();
            var viewModel = new GradeList
            {
                Grades = _mapper.Map<IEnumerable<Grade>>(grades)
            };
            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetGradesMapper : Profile
    {
        public GetGradesMapper()
        {
            CreateMap<Grade, GradeList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Grade With Pagination"
#region "Query"
    public sealed record GetGradesPaginationQuery(Expression<Func<Grade, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<Grade>>;
#endregion
#region "Handler"
    public sealed class GetGradesPaginationQueryHandler : IRequestHandler<GetGradesPaginationQuery, PaginatedList<Grade>>
    {
        private readonly IDataContext _context;
    
        public GetGradesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Grade>> Handle(GetGradesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Grades.AsQueryable().Where(b => b.DeletedAt == null);
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

            var grades = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
            grades.Items.ForEach(x =>
            {
                x.Company = _context.Companies.FirstOrDefault(f => f.Key == x.CompanyKey);
            });
            return await Task.FromResult(grades);
        }
    }
#endregion
#endregion

#region "Get By Id Grade"
#region "Query"
    public sealed record GetGradeQuery(Guid Key) : IRequest<GradeForm>;
#endregion
#region "Handler"
    public sealed class GetGradeQueryHandler : IRequestHandler<GetGradeQuery, GradeForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetGradeQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<GradeForm> Handle(GetGradeQuery request, CancellationToken cancellationToken)
        {
            var grade = await _context.Grades.FirstOrDefaultAsync(b => b.Key == request.Key);
            if (grade == null)
            {
                throw new InvalidOperationException("Grade not found or has been deleted.");
            }

            var company = (grade.CompanyKey != Guid.Empty)
                                ? await _context.Companies.FirstOrDefaultAsync(c => c.Key == grade.CompanyKey) : null;
            grade.Company = company;

            return _mapper.Map<GradeForm>(grade);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetGradeMapper : Profile
    {
        public GetGradeMapper()
        {
            CreateMap<Grade, GradeForm>().ReverseMap()
                    .ForMember(dest => dest.CompanyKey, opt => opt.MapFrom(src => src.CompanyKey))
                    .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company));
        }
    }
#endregion
#endregion

#region "Save Grade"
#region "Command"
    public sealed record SaveGradeCommand(GradeDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveGradeCommandHandler : IRequestHandler<SaveGradeCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<GradeDto> _validator;

        public SaveGradeCommandHandler(IDataContext context, IMapper mapper, IValidator<GradeDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }
        public async Task<Result> Handle(SaveGradeCommand command, CancellationToken cancellationToken)
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
                    //Create Grade
                    var grade = _mapper.Map<Grade>(command.Form);
                    grade.Key = Guid.NewGuid();

                    _context.Grades.Add(grade);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var grade = _mapper.Map<Grade>(command.Form);
                    if (grade == null)
                    {
                        throw new InvalidOperationException("Grade not found.");
                    }
                    _context.Grades.Update(grade);
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
    public sealed class SaveGradeMapper : Profile
    {
        public SaveGradeMapper()
        {
            CreateMap<Grade, GradeDto>().ReverseMap();
            CreateMap<Grade, SaveGradeCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Grade"
#region "Command"
    public sealed record DeleteGradeCommand(Guid Key) : IRequest<Result<Grade>>;
#endregion
#region "Handler"
    public sealed class DeleteGradeCommandHandler : IRequestHandler<DeleteGradeCommand, Result<Grade>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeleteGradeCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Grade>> Handle(DeleteGradeCommand command, CancellationToken cancellationToken)
        {
            var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Key == command.Key);

            try
            {
                if (grade == null)
                {
                    throw new InvalidOperationException("Grade not found.");
                }

                _mapper.Map<Grade>(grade);
                _context.Grades.Remove(grade);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Grade>.Failure(new[] { ex.Message });
            }
            return Result<Grade>.Success(grade);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteGradeMapper : Profile
    {
        public DeleteGradeMapper()
        {
            CreateMap<Grade, DeleteGradeCommand>().ReverseMap();
        }
    }
#endregion
#endregion
