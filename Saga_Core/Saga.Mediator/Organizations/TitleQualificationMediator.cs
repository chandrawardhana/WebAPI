using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.ViewModels.Organizations;
using Saga.Domain.ViewModels.Employees;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using System.Linq.Expressions;
using Saga.Domain.Dtos.Organizations;
using Saga.DomainShared;
using FluentValidation;
using FluentValidation.Results;
using Saga.Persistence.Models;

namespace Saga.Mediator.Organizations.TitleQualificationMediator;

#region "Get List Title Qualification With Pagination"
#region "Query"
    public sealed record GetTitleQualificationsPaginationQuery(Expression<Func<Title, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<TitleQualificationItem>>;
#endregion
#region "Handler"
    public sealed class GetTitleQualificationsPaginationQueryHandler : IRequestHandler<GetTitleQualificationsPaginationQuery, PaginatedList<TitleQualificationItem>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetTitleQualificationsPaginationQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<TitleQualificationItem>> Handle(GetTitleQualificationsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from ti in _context.Titles
                          join com in _context.Companies on ti.CompanyKey equals com.Key
                          join tq in _context.TitleQualifications on ti.Key equals tq.TitleKey into tqGroup
                          from tq in tqGroup.DefaultIfEmpty()
                          join ed in _context.Educations on tq.EducationKey equals ed.Key into edGroup
                          from ed in edGroup.DefaultIfEmpty()
                          join pos in _context.Positions on tq.PositionKey equals pos.Key into posGroup
                          from pos in posGroup.DefaultIfEmpty()
                          where ti.DeletedAt == null
                          select new
                          {
                              Title = ti,
                              Company = com,
                              TitleQualification = tq,
                              Education = ed,
                              Position = pos
                          };

            string search = request.pagination.Find;

            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(x => EF.Functions.ILike(x.Title.Code, $"%{search}%") || EF.Functions.ILike(x.Title.Name, $"%{search}%") || EF.Functions.ILike(x.Company.Name, $"%{search}%"));
            }

            // Applying additional filters from 'wheres'
            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x.Title));
            }

            var result = await queries
            .Select(x => new TitleQualificationItem
            {
                Key = x.TitleQualification.Key,
                TitleKey = x.Title.Key,
                Code = x.Title.Code,
                Name = x.Title.Name,
                CompanyName = x.Company.Name,
            })
            .PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);


            return await Task.FromResult(result);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetTitleQualificationsMapper : Profile
    {
        public GetTitleQualificationsMapper() 
        {
            CreateMap<TitleQualification, TitleQualificationItem>()
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Key))
            .ForMember(dest => dest.TitleKey, opt => opt.MapFrom(src => src.Title.Key))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Title.Code))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title.Name))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Title.Company.Name));
        }
    }
#endregion
#endregion

#region "Get By Id Title Qualification"
#region "Query"
    public sealed record GetTitleQualificationQuery(Guid TitleKey) : IRequest<TitleQualificationForm>;
#endregion
#region "Handler"
    public sealed class GetTitleQualificationQueryHandler : IRequestHandler<GetTitleQualificationQuery, TitleQualificationForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetTitleQualificationQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TitleQualificationForm> Handle(GetTitleQualificationQuery request, CancellationToken cancellationToken)
        {
            var titleQualification = await (from tq in _context.TitleQualifications
                                            join t in _context.Titles on tq.TitleKey equals t.Key
                                            join ed in _context.Educations on tq.EducationKey equals ed.Key
                                            join p in _context.Positions on tq.PositionKey equals p.Key
                                            where tq.TitleKey == request.TitleKey
                                            select new TitleQualificationForm
                                            {
                                                Key = tq.Key,
                                                TitleKey = tq.TitleKey,
                                                Title = new TitleForm
                                                {
                                                    Code = t.Code,
                                                    Name = t.Name
                                                },
                                                EducationKey = tq.EducationKey,
                                                Education = new EducationForm
                                                {
                                                    Code = ed.Code,
                                                    Name = ed.Name
                                                },
                                                SkillKeys = tq.SkillKeys,
                                                LanguageKeys = tq.LanguageKeys,
                                                PositionKey = tq.PositionKey,
                                                Position = new PositionForm
                                                {
                                                    Code = p.Code,
                                                    Name = p.Name
                                                },
                                                MinExperience = tq.MinExperience
                                            }).FirstOrDefaultAsync();
            if (titleQualification == null)
            {
                throw new Exception("Title Qualification Not Found");
            }

            var selectedSkills = await _context.Skills.Where(x => titleQualification.SkillKeys.Contains(x.Key))
            .Select(s => new SkillForm
            {
                Key = s.Key,
                Name = s.Name
            }).ToListAsync();

            var selectedLanguages = await _context.Languages.Where(x => titleQualification.LanguageKeys.Contains(x.Key))
            .Select(l => new LanguageForm
            {
                Key = l.Key,
                Name = l.Name
            }).ToListAsync();

            var titleQualificationForm = _mapper.Map<TitleQualificationForm>(titleQualification);
            titleQualificationForm.Skills = selectedSkills;
            titleQualificationForm.Languages = selectedLanguages;

            return titleQualificationForm;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetTitleQualificationMapper : Profile
    {
        public GetTitleQualificationMapper()
        {
            CreateMap<TitleQualification, TitleQualificationForm>().ReverseMap()
                     .ForMember(dest => dest.TitleKey, opt => opt.MapFrom(src => src.TitleKey))
                     .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                     .ForMember(dest => dest.EducationKey, opt => opt.MapFrom(src => src.EducationKey))
                     .ForMember(dest => dest.Education, opt => opt.MapFrom(src => src.Education))
                     .ForMember(dest => dest.SkillKeys, opt => opt.MapFrom(src => src.SkillKeys))
                     .ForMember(dest => dest.LanguageKeys, opt => opt.MapFrom(src => src.LanguageKeys))
                     .ForMember(dest => dest.PositionKey, opt => opt.MapFrom(src => src.PositionKey))
                     .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                     .ForMember(dest => dest.MinExperience, opt => opt.MapFrom(src => src.MinExperience));
        }
    }
#endregion
#endregion

#region "Save Title Qualification"
#region "Command"
    public sealed record SaveTitleQualificationCommand(TitleQualificationDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveTitleQualificationCommandHandler : IRequestHandler<SaveTitleQualificationCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<TitleQualificationDto> _validator;

        public SaveTitleQualificationCommandHandler(IDataContext context, IMapper mapper, IValidator<TitleQualificationDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveTitleQualificationCommand command, CancellationToken cancellationToken)
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
                    //Create Title Qualification
                    var titleQualification = _mapper.Map<TitleQualification>(command.Form);
                    titleQualification.Key = Guid.NewGuid();

                    _context.TitleQualifications.Add(titleQualification);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                } 
                else
                {
                    var titleQualification = await _context.TitleQualifications.FirstOrDefaultAsync(x => x.Key == command.Form.Key) 
                                ?? throw new Exception("Title Qualification Not Found");

                    _mapper.Map(command.Form, titleQualification);

                    _context.TitleQualifications.Update(titleQualification);
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
    public sealed class SaveTitleQualificationMapper : Profile
    {
        public SaveTitleQualificationMapper() 
        { 
            CreateMap<TitleQualification, TitleQualificationDto>().ReverseMap();
            CreateMap<TitleQualification, SaveTitleQualificationCommand>().ReverseMap();
        }
    }
#endregion
#endregion
