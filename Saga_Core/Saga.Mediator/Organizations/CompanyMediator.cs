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
using Saga.Mediator.Systems.AssetMediator;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Organizations.CompanyMediator;

#region "Get List Company"
#region "Query"
    public sealed record GetCompaniesQuery(Expression<Func<Company, bool>>[] wheres) : IRequest<CompanyList>;
#endregion
#region "Handler"
    public sealed class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, CompanyList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetCompaniesQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CompanyList> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Companies.AsQueryable().Where(b => b.DeletedAt == null);
            request.wheres.ToList()
                          .ForEach(x =>
                          {
                              queries = queries.Where(x);
                          });
            var companies = await queries.ToListAsync();
            var viewModel = new CompanyList
            {
                Companies = _mapper.Map<IEnumerable<Company>>(companies)
            };
            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetCompaniesMapper : Profile
    {
        public GetCompaniesMapper()
        {
            CreateMap<Company, CompanyList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Company With Pagination"
#region "Query"
    public sealed record GetCompaniesPaginationQuery(Expression<Func<Company, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<Company>>;
#endregion
#region "Handler"
    public sealed class GetCompaniesPaginationQueryHandler : IRequestHandler<GetCompaniesPaginationQuery, PaginatedList<Company>>
    {
        private readonly IDataContext _context;

        public GetCompaniesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Company>> Handle(GetCompaniesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Companies.AsQueryable().Where(b => b.DeletedAt == null);
            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Address, $"%{search}%"));
            }
            request.wheres.ToList()
                          .ForEach(x =>
                          {
                              queries = queries.Where(x);
                          });
            var companies = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            companies.Items.ForEach(x =>
            {
                x.Country = _context.Countries.FirstOrDefault(f => f.Key == x.CountryKey);
                x.Province = _context.Provinces.FirstOrDefault(f => f.Key == x.ProvinceKey);
                x.City = _context.Cities.FirstOrDefault(f => f.Key == x.CityKey);
                x.Bank = _context.Banks.FirstOrDefault(f => f.Key == x.BankKey);
            });

            return await Task.FromResult(companies);
        }
    }
#endregion
#endregion

#region "Get By Id Company"
#region "Query"
    public sealed record GetCompanyQuery(Guid Key) : IRequest<CompanyForm>;
#endregion
#region "Handler"
    public sealed class GetCompanyQueryHandler : IRequestHandler<GetCompanyQuery, CompanyForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetCompanyQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CompanyForm> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Key == request.Key);
            if (company == null)
            {
                throw new InvalidOperationException("Company not found or has been deleted.");
            }

            var country = (company.CountryKey != Guid.Empty)
                                  ? await _context.Countries.FirstOrDefaultAsync(c => c.Key == company.CountryKey) : null;

            var province = (company.ProvinceKey != Guid.Empty)
                                  ? await _context.Provinces.FirstOrDefaultAsync(c => c.Key == company.ProvinceKey) : null;

            var city = (company.CityKey != Guid.Empty)
                              ? await _context.Cities.FirstOrDefaultAsync(c => c.Key == company.CityKey) : null;

            var bank = (company.BankKey != Guid.Empty)
                               ? await _context.Banks.FirstOrDefaultAsync(c => c.Key == company.BankKey) : null;

            company.Country = country;
            company.Province = province;
            company.City = city;
            company.Bank = bank;

            return _mapper.Map<CompanyForm>(company);
        }
    }
#endregion
#region "Mapper"
    public sealed class GetCompanyMapper : Profile
    {
        public GetCompanyMapper()
        {
            CreateMap<Company, CompanyForm>().ReverseMap()
                      .ForMember(dest => dest.CountryKey, opt => opt.MapFrom(src => src.CountryKey))
                      .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                      .ForMember(dest => dest.ProvinceKey, opt => opt.MapFrom(src => src.ProvinceKey))
                      .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province))
                      .ForMember(dest => dest.CityKey, opt => opt.MapFrom(src => src.CityKey))
                      .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                      .ForMember(dest => dest.BankKey, opt => opt.MapFrom(src => src.BankKey))
                      .ForMember(dest => dest.Bank, opt => opt.MapFrom(src => src.Bank));
        }
    }
#endregion
#endregion

#region "Save Company"
#region "Command"
    public sealed record SaveCompanyCommand(CompanyDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveCompanyCommandHandler : IRequestHandler<SaveCompanyCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<CompanyDto> _validator;
        private readonly IMediator _mediator;

        public SaveCompanyCommandHandler(IDataContext context, IMapper mapper, IValidator<CompanyDto> validator, IMediator mediator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
            _mediator = mediator;
        }

        public async Task<Result> Handle(SaveCompanyCommand command, CancellationToken cancellationToken)
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

                if (command.Form.Logo != null && command.Form.Logo.Length > 0)
                {
                    var logoUpload = await _mediator.Send(new UploadFileCommand(command.Form.Logo), cancellationToken);
                    if (!logoUpload.Succeeded)
                        return Result.Failure(logoUpload.Errors);

                    command.Form.LogoKey = logoUpload.Value.Key;
                }

                if (command.Form.Key == Guid.Empty || command.Form.Key == null)
                {
                    // Create Company
                    var company = _mapper.Map<Company>(command.Form);
                    company.Key = Guid.NewGuid();

                    _context.Companies.Add(company);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var company = _mapper.Map<Company>(command.Form);
                    if (company == null)
                    {
                        throw new Exception("Company Not Found");
                    }
                    _context.Companies.Update(company);
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
    public sealed class SaveCompanyMapper : Profile
    {
        public SaveCompanyMapper()
        {
            CreateMap<Company, CompanyDto>().ReverseMap()
                        .ForMember(dest => dest.AssetKey, opt => opt.MapFrom(src => src.LogoKey));
            CreateMap<Company, SaveCompanyCommand>().ReverseMap()
                        .ForMember(dest => dest.AssetKey, opt => opt.MapFrom(src => src.Form.LogoKey));
    }
    }
#endregion
#endregion

#region "Delete Company"
#region "Command"
    public sealed record DeleteCompanyCommand(Guid Key) : IRequest<Result<Company>>;
#endregion
#region "Handler"
    public sealed class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Result<Company>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public DeleteCompanyCommandHandler(IDataContext context, IMapper mapper, IMediator mediator)
        {
            _context = context;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<Result<Company>> Handle(DeleteCompanyCommand command, CancellationToken cancellationToken)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Key == command.Key);

            try
            {
                if (company == null)
                {
                    throw new Exception("Company Not Found");
                }

                _mapper.Map<Company>(company);

                if (company.AssetKey != Guid.Empty || company.AssetKey.HasValue)
                {
                    var logoDelete = await _mediator.Send(new DeleteFileCommand((Guid)company.AssetKey), cancellationToken);
                    if (!logoDelete.Succeeded)
                        throw new Exception(logoDelete.Errors.FirstOrDefault());
                }

                _context.Companies.Remove(company);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Company>.Failure(new[] { ex.Message });
            }
            return Result<Company>.Success(company);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteCompanyMapper : Profile
    {
        public DeleteCompanyMapper()
        {
            CreateMap<Company, DeleteCompanyCommand>().ReverseMap();
        }
    }
#endregion
#endregion
