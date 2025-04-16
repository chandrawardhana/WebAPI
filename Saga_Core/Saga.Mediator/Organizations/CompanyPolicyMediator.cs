using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Organizations;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Organizations;
using Saga.DomainShared;
using Saga.DomainShared.Interfaces;
using Saga.DomainShared.Models;
using Saga.Mediator.Services;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Organizations.CompanyPolicyMediator;

#region "Get List Company Policy"
#region "Query"
    public sealed record GetCompanyPoliciesQuery(Expression<Func<CompanyPolicy, bool>>[] wheres) : IRequest<CompanyPolicyList>;
#endregion
#region "Handler"
    public sealed class GetCompanyPoliciesQueryHandler : IRequestHandler<GetCompanyPoliciesQuery, CompanyPolicyList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetCompanyPoliciesQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CompanyPolicyList> Handle(GetCompanyPoliciesQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.CompanyPolicies.AsQueryable().Where(b => b.DeletedAt == null);
            request.wheres.ToList()
                                 .ForEach(x =>
                                 {
                                     queries = queries.Where(x);
                                 });

            var companyPolicies = await queries.ToListAsync();
            companyPolicies.ForEach(x =>
            {
                x.Company = _context.Companies.FirstOrDefault(f => f.Key == x.CompanyKey);
                x.Organization = _context.Organizations.FirstOrDefault(f => f.Key == x.OrganizationKey);
            });

            var viewModel = new CompanyPolicyList
            {
                CompanyPolicies = _mapper.Map<IEnumerable<CompanyPolicy>>(companyPolicies)
            };
            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetCompanyPoliciesMapper : Profile
    {
        public GetCompanyPoliciesMapper()
        {
            CreateMap<CompanyPolicy, CompanyPolicyList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Company Policy With Pagination"
#region "Query"
    public sealed record GetCompanyPoliciesPaginationQuery(Expression<Func<CompanyPolicy, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<CompanyPolicy>>;
#endregion
#region "Handler"
    public sealed class GetCompanyPoliciesPaginationQueryHandler : IRequestHandler<GetCompanyPoliciesPaginationQuery, PaginatedList<CompanyPolicy>>
    {
        private readonly IDataContext _context;

        public GetCompanyPoliciesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<CompanyPolicy>> Handle(GetCompanyPoliciesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from cp in _context.CompanyPolicies
                          join com in _context.Companies on cp.CompanyKey equals com.Key
                          join org in _context.Organizations on cp.OrganizationKey equals org.Key
                          where cp.DeletedAt == null
                          select new
                          {
                              CompanyPolicy = cp,
                              Company = com,
                              Organization = org
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(x =>
                    EF.Functions.ILike(x.Company.Name, $"%{search}%") ||
                    EF.Functions.ILike(x.Organization.Name, $"%{search}%") ||
                    EF.Functions.ILike(x.Organization.Code, $"%{search}%") ||
                    // Effective date search
                    EF.Functions.ILike(DateFunctions.ToChar(x.CompanyPolicy.EffectiveDate, "DD TMMonth YYYY"), $"%{search}%") ||
                    EF.Functions.ILike(DateFunctions.ToChar(x.CompanyPolicy.EffectiveDate, "DD"), $"%{search}%") ||
                    EF.Functions.ILike(DateFunctions.ToChar(x.CompanyPolicy.EffectiveDate, "TMMonth"), $"%{search}%") ||
                    EF.Functions.ILike(DateFunctions.ToChar(x.CompanyPolicy.EffectiveDate, "YYYY"), $"%{search}%") ||
                    // ExpiredDate searches
                    (x.CompanyPolicy.ExpiredDate.HasValue && (
                        EF.Functions.ILike(DateFunctions.ToChar(x.CompanyPolicy.ExpiredDate.Value, "DD TMMonth YYYY"), $"%{search}%") ||
                        EF.Functions.ILike(DateFunctions.ToChar(x.CompanyPolicy.ExpiredDate.Value, "DD"), $"%{search}%") ||
                        EF.Functions.ILike(DateFunctions.ToChar(x.CompanyPolicy.ExpiredDate.Value, "TMMonth"), $"%{search}%") ||
                        EF.Functions.ILike(DateFunctions.ToChar(x.CompanyPolicy.ExpiredDate.Value, "YYYY"), $"%{search}%")
                    ))
                );

            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x.CompanyPolicy));
            }

            var companyPolicies = await queries.Select(x => new CompanyPolicy
            {
                Key = x.CompanyPolicy.Key,
                CompanyKey = x.Company.Key,
                Company = x.Company,
                OrganizationKey = x.Organization.Key,
                Organization = x.Organization,
                EffectiveDate = x.CompanyPolicy.EffectiveDate,
                ExpiredDate = x.CompanyPolicy.ExpiredDate,
                Policy = x.CompanyPolicy.Policy
            }).PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            return await Task.FromResult(companyPolicies);
        }
    }
#endregion
#endregion

#region "Get By Id Company Policy"
#region "Query"
    public sealed record GetCompanyPolicyQuery(Guid Key) : IRequest<CompanyPolicyForm>;
#endregion
#region "Handler"
    public sealed class GetCompanyPolicyQueryHandler : IRequestHandler<GetCompanyPolicyQuery, CompanyPolicyForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetCompanyPolicyQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CompanyPolicyForm> Handle(GetCompanyPolicyQuery request, CancellationToken cancellationToken)
        {
            var companyPolicy = await (from cp in _context.CompanyPolicies
                                       join com in _context.Companies on cp.CompanyKey equals com.Key
                                       join org in _context.Organizations on cp.OrganizationKey equals org.Key
                                       where cp.Key == request.Key && cp.DeletedAt == null
                                       select new CompanyPolicyForm
                                       {
                                           Key = cp.Key,
                                           CompanyKey = cp.CompanyKey,
                                           Company = com,
                                           OrganizationKey = cp.OrganizationKey,
                                           Organization = org,
                                           EffectiveDate = cp.EffectiveDate,
                                           ExpiredDate = cp.ExpiredDate,
                                           Policy = cp.Policy
                                       }).FirstOrDefaultAsync();

            if (companyPolicy == null)
            {
                throw new InvalidOperationException("Company Policy not found or has been deleted.");
            }

            return _mapper.Map<CompanyPolicyForm>(companyPolicy);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetCompanyPolicyMapper : Profile
    {
        public GetCompanyPolicyMapper()
        {
            CreateMap<CompanyPolicy, CompanyPolicyForm>().ReverseMap()
                        .ForMember(dest => dest.CompanyKey, opt => opt.MapFrom(src => src.CompanyKey))
                        .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
                        .ForMember(dest => dest.OrganizationKey, opt => opt.MapFrom(src => src.OrganizationKey))
                        .ForMember(dest => dest.Organization, opt => opt.MapFrom(src => src.Organization));
        }
    }
#endregion
#endregion

#region "Generate Report Company Policy"
#region "Query"
    public sealed record GenerateCompanyPolicyReportQuery(CompanyPolicyReportDto report) : IRequest<byte[]>;
#endregion
#region "Handler"
    public sealed class GenerateCompanyPolicyReportQueryHandler : IRequestHandler<GenerateCompanyPolicyReportQuery, byte[]>
    {
        private readonly IOrganizationRepository _repository;
        private readonly IDocumentGenerator _documentGenerator;

        public GenerateCompanyPolicyReportQueryHandler(IOrganizationRepository repository, IDocumentGenerator documentGenerator)
        {
            _repository = repository;
            _documentGenerator = documentGenerator;
        }

        public async Task<byte[]> Handle(GenerateCompanyPolicyReportQuery request, CancellationToken cancellationToken)
        {
            var companyPolicies = await _repository.GetCompanyPolicyReportQuery(request.report);
            var companyPolicyReport = new CompanyPolicyReport
            {
                CompanyPolicies = companyPolicies,
                OrganizationKey = request.report.OrganizationKey,
                EffectiveDate = request.report.EffectiveDate,
                DocumentGeneratorFormat = request.report.DocumentGeneratorFormat
            };
            var document = await _documentGenerator.GenerateDocumentAsync("CompanyPolicy", companyPolicyReport, request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Pdf);
            
            return document;
        }
    }
#endregion
#endregion

#region "Get Company Policy Report Query Preview"
#region "Query"
    public sealed record GetCompanyPolicyReportQuery(CompanyPolicyReportDto report) : IRequest<CompanyPolicyReport>;
#endregion
#region "Handler"
    public sealed class GetCompanyPolicyReportQueryHandler(IOrganizationRepository _repo) : IRequestHandler<GetCompanyPolicyReportQuery, CompanyPolicyReport>
    {
        public async Task<CompanyPolicyReport> Handle(GetCompanyPolicyReportQuery request, CancellationToken cancellationToken)
        {
            var companyPolicies = await _repo.GetCompanyPolicyReportQuery(request.report);
            var viewModel = new CompanyPolicyReport
            {
                CompanyPolicies = companyPolicies
            };

            return viewModel;
        }
    }
#endregion
#endregion

#region "Save Company Policy"
#region "Command"
public sealed record SaveCompanyPolicyCommand(CompanyPolicyDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveCompanyPolicyCommandHandler : IRequestHandler<SaveCompanyPolicyCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<CompanyPolicyDto> _validator;

        public SaveCompanyPolicyCommandHandler(IDataContext context, IMapper mapper, IValidator<CompanyPolicyDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveCompanyPolicyCommand command, CancellationToken cancellationToken)
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

                if (command.Form.Key == Guid.Empty || command.Form == null)
                {
                    //Create CompanyPolicy
                    var companyPolicy = _mapper.Map<CompanyPolicy>(command.Form);
                    companyPolicy.Key = Guid.NewGuid();

                    _context.CompanyPolicies.Add(companyPolicy);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var companyPolicy = _mapper.Map<CompanyPolicy>(command.Form);
                    if (companyPolicy == null)
                    {
                        throw new InvalidOperationException("Company Policy not found.");
                    }
                    _context.CompanyPolicies.Update(companyPolicy);
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
    public sealed class SaveCompanyPolicyMapper : Profile
    {
        public SaveCompanyPolicyMapper()
        {
            CreateMap<CompanyPolicy, CompanyPolicyDto>().ReverseMap();
            CreateMap<CompanyPolicy, SaveCompanyPolicyCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Company Policy"
#region "Command"
    public sealed record DeleteCompanyPolicyCommand(Guid Key) : IRequest<Result<CompanyPolicy>>;
#endregion
#region "Handler"
    public sealed class DeleteCompanyPolicyCommandHandler : IRequestHandler<DeleteCompanyPolicyCommand, Result<CompanyPolicy>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeleteCompanyPolicyCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<CompanyPolicy>> Handle(DeleteCompanyPolicyCommand command, CancellationToken cancellationToken)
        {
            var companyPolicy = await _context.CompanyPolicies.FirstOrDefaultAsync(c => c.Key == command.Key);

            try
            {
                if (companyPolicy == null)
                {
                    throw new Exception("Company Policy Not Found");
                }
                _mapper.Map<CompanyPolicy>(command);

                _context.CompanyPolicies.Remove(companyPolicy);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<CompanyPolicy>.Failure(new[] { ex.Message });
            }
            return Result<CompanyPolicy>.Success(companyPolicy);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteCompanyPolicyMapper : Profile
    {
        public DeleteCompanyPolicyMapper()
        {
            CreateMap<CompanyPolicy, DeleteCompanyPolicyCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Generate Company Policy Report From CSHTML"
#region "Query"
    public sealed record GenerateCompanyPolicyReportCSHTMLQuery(CompanyPolicyReportDto report) : IRequest<byte[]>;
#endregion
#region "Handler"
    public sealed class GenerateCompanyPolicyReportCSHTMLQueryHandler : IRequestHandler<GenerateCompanyPolicyReportCSHTMLQuery, byte[]>
    {
        private readonly IOrganizationRepository _repository;
        private readonly IDocumentGenerator _documentGenerator;

        public GenerateCompanyPolicyReportCSHTMLQueryHandler(IOrganizationRepository repository, IDocumentGenerator documentGenerator)
        {
            _repository = repository;
            _documentGenerator = documentGenerator;
        }

        public async Task<byte[]> Handle(GenerateCompanyPolicyReportCSHTMLQuery request, CancellationToken cancellationToken)
        {
            var companyPolicies = await _repository.GetCompanyPolicyReportQuery(request.report);
            var companyPolicyReport = new CompanyPolicyReport
            {
                CompanyPolicies = companyPolicies,
                OrganizationKey = request.report.OrganizationKey,
                EffectiveDate = request.report.EffectiveDate,
                DocumentGeneratorFormat = request.report.DocumentGeneratorFormat
            };

            var htmlTemplate = await _documentGenerator.GetCompanyPolicyReportHTML(companyPolicyReport);

            var document = await _documentGenerator.GenerateCompanyPolicyReportPDF(htmlTemplate);
        
            return document;
        }
    }
#endregion
#endregion
