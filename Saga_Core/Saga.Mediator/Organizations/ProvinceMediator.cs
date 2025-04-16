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

namespace Saga.Mediator.Organizations.ProvinceMediator;


#region "Get List Province"
#region "Query"
    public sealed record GetProvincesQuery(Expression<Func<Province, bool>>[] wheres) : IRequest<ProvinceList>;
#endregion
#region "Handler"
    public sealed class GetProvincesQueryHandler : IRequestHandler<GetProvincesQuery, ProvinceList>
    {
        private readonly IDataContext _context;

        public GetProvincesQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<ProvinceList> Handle(GetProvincesQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Provinces.AsQueryable().Where(b => b.DeletedAt == null);
        
            request.wheres.ToList()
                  .ForEach(x =>
                  {
                      queries = queries.Where(x);
                  });

            var provinces = await queries.ToListAsync();

            var viewModel = new ProvinceList
            {
                Provinces = provinces.Select(province => province.ConvertToViewModelProvinceListItem())
            };
            return viewModel;
        }
    }
#endregion
#endregion

#region "Get List Province With Pagination"
#region "Query"
    public sealed record GetProvincesPaginationQuery(Expression<Func<Province, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<Province>>;
#endregion
#region "Handler"
    public sealed class GetProvincesPaginationQueryHandler : IRequestHandler<GetProvincesPaginationQuery, PaginatedList<Province>>
    {
        private readonly IDataContext _context;

        public GetProvincesPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<Province>> Handle(GetProvincesPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = _context.Provinces.AsQueryable().Where(b => b.DeletedAt == null);
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

            var provinces = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            provinces.Items.ForEach(x =>
            {
                x.Country = _context.Countries.FirstOrDefault(f => f.Key == x.CountryKey);
            });

            return await Task.FromResult(provinces);
        }
    }
#endregion
#endregion

#region "Get By Id Province"
#region "Query"
    public sealed record GetProvinceQuery(Guid Key) : IRequest<ProvinceForm>;
#endregion
#region "Handler"
    public sealed class GetProvinceQueryHandler : IRequestHandler<GetProvinceQuery, ProvinceForm>
    {
        private readonly IDataContext _context;

        public GetProvinceQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<ProvinceForm> Handle(GetProvinceQuery request, CancellationToken cancellationToken)
        {
            var province = await _context.Provinces.FirstOrDefaultAsync(p => p.Key == request.Key);

            if (province == null)
            {
                throw new InvalidOperationException("Province not found or has been deleted.");
            }

            var country = (province.CountryKey != Guid.Empty) 
                                ? await _context.Countries.FirstOrDefaultAsync(c => c.Key == province.CountryKey) : null;
            
            province.Country = country;

            return province.ConvertToViewModelProvinceForm(); 
        }
    }
#endregion
#endregion

#region "Save Province"
#region "Command"
    public sealed record SaveProvinceCommand(ProvinceDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveProvinceHandler : IRequestHandler<SaveProvinceCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<ProvinceDto> _validator;

        public SaveProvinceHandler(IDataContext context, IValidator<ProvinceDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveProvinceCommand command, CancellationToken cancellationToken)
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

                var province = command.Form.ConvertToEntity();
                if (province.Key == Guid.Empty)
                {
                    //Create Province
                    province.Key = Guid.NewGuid();
                }

                //Check if province exists
                var existingProvince = await _context.Provinces
                                                     .FirstOrDefaultAsync(p => p.Key == province.Key && p.DeletedAt == null, cancellationToken);

                if (existingProvince == null)
                {
                    //Add new Province
                    _context.Provinces.Add(province);
                }
                else
                {
                    //Update existing Province
                    province.CreatedAt = existingProvince.CreatedAt;
                    province.CreatedBy = existingProvince.CreatedBy;
                    _context.Provinces.Entry(existingProvince).CurrentValues.SetValues(province);
                }

                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }
            return Result.Success();
        }
    }
#endregion
#endregion

#region "Delete Province"
#region "Command"
    public sealed record DeleteProvinceCommand(Guid Key) : IRequest<Result<Province>>;
#endregion
#region "Handler"
    public sealed class DeleteProvinceHandler : IRequestHandler<DeleteProvinceCommand, Result<Province>>
    {
        private readonly IDataContext _context;

        public DeleteProvinceHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<Province>> Handle(DeleteProvinceCommand command, CancellationToken cancellationToken)
        {
            var province = await _context.Provinces.FirstOrDefaultAsync(p => p.Key == command.Key);
            try
            {
                if (province == null)
                {
                    throw new Exception("Province Not Found");
                }
            
                _context.Provinces.Remove(province);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<Province>.Failure(new[] { ex.Message });
            }

            return Result<Province>.Success(province);
        }
    }
#endregion
#endregion
