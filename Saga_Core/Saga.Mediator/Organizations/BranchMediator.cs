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

namespace Saga.Mediator.Organizations.BranchMediator;

#region "Get List Branch"
#region "Query"
    public sealed record GetBranchesQuery(Expression<Func<Branch, bool>>[] wheres) : IRequest<BranchList>;
#endregion
#region "Handler"
    public sealed class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, BranchList>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetBranchesQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BranchList> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Branches.AsQueryable().Where(b => b.DeletedAt == null);
            request.wheres.ToList()
                             .ForEach(x =>
                             {
                                 queries = queries.Where(x);
                             });
            var branches = await queries.ToListAsync();
            var viewModel = new BranchList
            {
                Branches = _mapper.Map<IEnumerable<Branch>>(branches)
            };
            return viewModel;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetBranchesMapper : Profile
    {
        public GetBranchesMapper()
        {
            CreateMap<Branch, BranchList>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Get List Branch With Pagination"
#region "Query"
    public sealed record GetBranchesPaginationQuery(Expression<Func<Branch, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<Branch>>;
#endregion
#region "Handler"
    public sealed class GetBranchesPaginationQueryHandler : IRequestHandler<GetBranchesPaginationQuery, PaginatedList<Branch>>
    {
        private readonly IDataContext _context;

        public GetBranchesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Branch>> Handle(GetBranchesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Branches.AsQueryable().Where(b => b.DeletedAt == null);
            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Address, $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%"));
            }

            request.wheres.ToList()
                          .ForEach(x =>
                          {
                              queries = queries.Where(x);
                          });

            var branches = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
            branches.Items.ForEach(x =>
            {
                x.Company = _context.Companies.FirstOrDefault(f => f.Key == x.CompanyKey);
                x.Country = _context.Countries.FirstOrDefault(f => f.Key == x.CountryKey);
                x.Province = _context.Provinces.FirstOrDefault(f => f.Key == x.ProvinceKey);
                x.City = _context.Cities.FirstOrDefault(f => f.Key == x.CityKey);
                x.Bank = _context.Banks.FirstOrDefault(f => f.Key == x.BankKey);
            });

            return await Task.FromResult(branches);
        }
    }
#endregion
#endregion

#region "Get By Id Branch"
#region "Query"
    public sealed record GetBranchQuery(Guid Key) : IRequest<BranchForm>;
#endregion
#region "Handler"
    public sealed class GetBranchQueryHandler : IRequestHandler<GetBranchQuery, BranchForm>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public GetBranchQueryHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BranchForm> Handle(GetBranchQuery request, CancellationToken cancellationToken)
        {
            var branch = await (from br in _context.Branches
                    join com in _context.Companies on br.CompanyKey equals com.Key
                    join cou in _context.Countries on br.CountryKey equals cou.Key
                    join prov in _context.Provinces on br.ProvinceKey equals prov.Key
                    join cit in _context.Cities on br.CityKey equals cit.Key
                    join bk in _context.Banks on br.BankKey equals bk.Key
                    where br.Key == request.Key
                    select new 
                    { 
                        Branch = br,
                        Company = com,
                        Country = cou,
                        Province = prov,
                        City = cit,
                        Bank = bk
                    }).FirstOrDefaultAsync();

            if (branch == null)
            {
                throw new InvalidOperationException("Branch not found or has been deleted.");
            }

            var branchForm = _mapper.Map<BranchForm>(branch.Branch);
            branchForm.Company = branch.Company;
            branchForm.Country = branch.Country;
            branchForm.Province = branch.Province;
            branchForm.City = branch.City;
            branchForm.Bank = branch.Bank;

            return branchForm;
        }
    }
#endregion
#region "Mapper"
    public sealed class GetBranchMapper : Profile
    {
        public GetBranchMapper()
        {
            CreateMap<Branch, BranchForm>().ReverseMap()
                      .ForMember(dest => dest.CompanyKey, opt => opt.MapFrom(src => src.CompanyKey))
                      .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Company))
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

#region "Save Branch"
#region "Command"
    public sealed record SaveBranchCommand(BranchDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveBranchCommandHandler : IRequestHandler<SaveBranchCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IValidator<BranchDto> _validator;

        public SaveBranchCommandHandler(IDataContext context, IMapper mapper, IMediator mediator, IValidator<BranchDto> validator)
        {
            _context = context;
            _mapper = mapper;
            _mediator = mediator;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveBranchCommand command, CancellationToken cancellationToken)
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
                    // Create Branch
                    var branch = _mapper.Map<Branch>(command.Form);
                    branch.Key = Guid.NewGuid();

                    _context.Branches.Add(branch);
                    var result = await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var branch = _mapper.Map<Branch>(command.Form);
                    if (branch == null)
                    {
                        throw new Exception("Branch Not Found");
                    }
                    _context.Branches.Update(branch);
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
    public sealed class SaveBranchMapper : Profile
    {
        public SaveBranchMapper()
        {
            CreateMap<Branch, BranchDto>().ReverseMap()
                    .ForMember(dest => dest.AssetKey, opt => opt.MapFrom(src => src.LogoKey));
            CreateMap<Branch, SaveBranchCommand>().ReverseMap()
                    .ForMember(dest => dest.AssetKey, opt => opt.MapFrom(src => src.Form.LogoKey));
        }
    }
#endregion
#endregion

#region "Delete Branch"
#region "Command"
    public sealed record DeleteBranchCommand(Guid Key) : IRequest<Result<Branch>>;
#endregion
#region "Handler"
    public sealed class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result<Branch>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public DeleteBranchCommandHandler(IDataContext context, IMapper mapper, IMediator mediator)
        {
            _context = context;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<Result<Branch>> Handle(DeleteBranchCommand command, CancellationToken cancellationToken)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Key == command.Key);

            try
            {
                if (branch == null)
                {
                    throw new Exception("Branch Not Found");
                }

                _mapper.Map<Branch>(branch);

                if (branch.AssetKey != Guid.Empty || branch.AssetKey != null)
                {
                    var logoDelete = await _mediator.Send(new DeleteFileCommand((Guid)branch.AssetKey), cancellationToken);
                    if (!logoDelete.Succeeded)
                        throw new Exception(logoDelete.Errors.FirstOrDefault());
                }

                _context.Branches.Remove(branch);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Branch>.Failure(new[] { ex.Message });
            }
            return Result<Branch>.Success(branch);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteBranchMapper : Profile
    {
        public DeleteBranchMapper()
        {
            CreateMap<Branch, DeleteBranchCommand>().ReverseMap();
        }
    }
#endregion
#endregion
