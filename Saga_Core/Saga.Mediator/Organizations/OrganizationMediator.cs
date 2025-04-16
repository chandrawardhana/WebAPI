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
using Saga.Mediator.Services;
using Saga.Persistence.Context;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Organizations.OrganizationMediator;

#region "Get List Organization"
#region "Query"
    public sealed record GetOrganizationsQuery(Expression<Func<Organization, bool>>[] wheres) : IRequest<OrganizationList>;
#endregion
#region "Handler"
    public sealed class GetOrganizationsQueryHandler(IOrganizationRepository _repo) : IRequestHandler<GetOrganizationsQuery, OrganizationList>
    {
        public async Task<OrganizationList> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
            => await _repo.GetAllOrganizations(request.wheres);
    }
#endregion
#endregion

#region "Get List Organization With Pagination"
#region "Query"
    public sealed record GetOrganizationsPaginationQuery(Expression<Func<Organization, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<Organization>>;
#endregion
#region "Handler"
    public sealed class GetOrganizationsPaginationQueryHandler(IOrganizationRepository _repo) : IRequestHandler<GetOrganizationsPaginationQuery, PaginatedList<Organization>>
    {
        public async Task<PaginatedList<Organization>> Handle(GetOrganizationsPaginationQuery request, CancellationToken cancellationToken)
            => await _repo.GetAllOrganizationsWithPagination(request.wheres, request.pagination);
    }
#endregion
#endregion

#region "Get By Id Organization"
#region "Query"
    public sealed record GetOrganizationQuery(Guid Key, Guid? ParentKey) : IRequest<OrganizationForm>;
#endregion
#region "Handler"
    public sealed class GetOrganizationQueryHandler(IOrganizationRepository _repo) : IRequestHandler<GetOrganizationQuery, OrganizationForm>
    {
        public async Task<OrganizationForm> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
            => await _repo.GetOrganization(request.Key, request.ParentKey);
    }
#endregion
#endregion

#region "Get List Parent Organization"
#region "Query"
    public sealed record GetParentOrganizationsQuery(string? searchString) : IRequest<IEnumerable<ParentList>>;
#endregion
#region "Handler"
    public sealed class GetParentOrganizationsQueryHandler : IRequestHandler<GetParentOrganizationsQuery, IEnumerable<ParentList>>
    {
        private readonly IDataContext _context;

        public GetParentOrganizationsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ParentList>> Handle(GetParentOrganizationsQuery request, CancellationToken cancellationToken)
        {

            var companies = await _context.Companies.Where(x => x.DeletedAt == null).ToListAsync();
            var masterOrganizations = await _context.Organizations.Where(x => x.DeletedAt == null).ToListAsync();
            // =============
            List<ParentList> masters = [];
            // get data company & save to memory
            masters.AddRange(companies.Select(x => new ParentList
            {
                Key = x.Key,
                Code = x.Code,
                Name = x.Name,
                ParentKey = Guid.Empty,
                Level = 0,
                Company = x
            }).ToList());

            masterOrganizations.ForEach(x =>
            {
                masters.Add(new ParentList()
                {
                    Key = x.Key,
                    Code = x.Code,
                    Name = x.Name,
                    ParentKey = x.ParentKey,
                    Level = x.Level,
                    Company = companies.FirstOrDefault(f => f.Key == x.CompanyKey),
                    //Parent =
                });
            });
            //===================

            if (!string.IsNullOrEmpty(request.searchString))
            {
                masters = masters.Where(b => b.Code.Contains(request.searchString) || b.Name.Contains(request.searchString) || b.Company.Name.Contains(request.searchString)).ToList();
            }
            return masters;
        }
    }
#endregion
#endregion

#region "Save Organization"
#region "Command"
public sealed record SaveOrganizationCommand(OrganizationDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveOrganizationCommandHandler : IRequestHandler<SaveOrganizationCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<OrganizationDto> _validator;
        private readonly IOrganizationRepository _organizationRepository;

        public SaveOrganizationCommandHandler(IDataContext context, IMapper mapper, IValidator<OrganizationDto> validator, IOrganizationRepository organizationRepository)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
            _organizationRepository = organizationRepository;
        }

        public async Task<Result> Handle(SaveOrganizationCommand command, CancellationToken cancellationToken)
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

                var organization = _mapper.Map<Organization>(command.Form);
                await _organizationRepository.SaveOrganizationAsync(organization, cancellationToken);
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
    public sealed class SaveOrganizationMapper : Profile
    {
        public SaveOrganizationMapper()
        {
            CreateMap<Organization, OrganizationDto>().ReverseMap();
            CreateMap<Organization, SaveOrganizationCommand>().ReverseMap();
        }
    }
#endregion
#endregion

#region "Delete Organization"
#region "Command"
    public sealed record DeleteOrganizationCommand(Guid Key) : IRequest<Result<Organization>>;
#endregion
#region "Handler"
    public sealed class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, Result<Organization>>
    {
        private readonly IDataContext _context;
        private readonly IMapper _mapper;

        public DeleteOrganizationCommandHandler(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<Organization>> Handle(DeleteOrganizationCommand command, CancellationToken cancellationToken)
        {
            var organization = await _context.Organizations.FirstOrDefaultAsync(c => c.Key == command.Key);

            try
            {
                if (organization == null)
                {
                    throw new Exception("Organization Not Found");
                }
                _mapper.Map<Organization>(command);

                _context.Organizations.Remove(organization);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Organization>.Failure(new[] { ex.Message });
            }
            return Result<Organization>.Success(organization);
        }
    }
#endregion
#region "Mapper"
    public sealed class DeleteOrganizationMapper : Profile
    {
        public DeleteOrganizationMapper()
        {
            CreateMap<Organization, DeleteOrganizationCommand>().ReverseMap();
        }
    }
#endregion
#endregion
