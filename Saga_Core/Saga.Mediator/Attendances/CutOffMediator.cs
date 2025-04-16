using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.CutOffMediator;

#region "Get List Cut Off"
#region "Query"
    public sealed record GetCutOffsQuery(Expression<Func<CutOff, bool>>[] wheres) : IRequest<CutOffList>;
#endregion
#region "Handler"
    public sealed class GetCutOffsQueryHandler : IRequestHandler<GetCutOffsQuery, CutOffList>
    {
        private readonly IDataContext _context;

        public GetCutOffsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<CutOffList> Handle(GetCutOffsQuery request, CancellationToken cancellationToken)
        {
            var queries = from cutoff in _context.CutOffs
                          join company in _context.Companies on cutoff.CompanyKey equals company.Key
                          where cutoff.DeletedAt == null
                          select new CutOff
                          {
                              Key = cutoff.Key,
                              CompanyKey = cutoff.CompanyKey,
                              YearPeriod = cutoff.YearPeriod,
                              Description = cutoff.Description,
                              JanStart = cutoff.JanStart,
                              JanEnd = cutoff.JanEnd,
                              FebStart = cutoff.FebStart,
                              FebEnd = cutoff.FebEnd,
                              MarStart = cutoff.MarStart,
                              MarEnd = cutoff.MarEnd,
                              AprStart = cutoff.AprStart,
                              AprEnd = cutoff.AprEnd,
                              MayStart = cutoff.MayStart,
                              MayEnd = cutoff.MayEnd,
                              JunStart = cutoff.JunStart,
                              JunEnd = cutoff.JunEnd,
                              JulStart = cutoff.JulStart,
                              JulEnd = cutoff.JulEnd,
                              AugStart = cutoff.AugStart,
                              AugEnd = cutoff.AugEnd,
                              SepStart = cutoff.SepStart,
                              SepEnd = cutoff.SepEnd,
                              OctStart = cutoff.OctStart,
                              OctEnd = cutoff.OctEnd,
                              NovStart = cutoff.NovStart,
                              NovEnd = cutoff.NovEnd,
                              DecStart = cutoff.DecStart,
                              DecEnd = cutoff.DecEnd,
                              Company = company
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(filter);
            }

            var cutOffs = await queries.ToListAsync();

            var viewModel = new CutOffList
            {
                CutOffs = cutOffs.Select(cutoff => cutoff.ConvertToViewModelCutOff())
            };

            return viewModel;
        }
    }
#endregion
#endregion

#region "Get List Cut Off With Pagination"
#region "Query"
    public sealed record GetCutOffsPaginationQuery(PaginationConfig pagination, Expression<Func<CutOff, bool>>[] wheres) : IRequest<PaginatedList<CutOff>>;
#endregion
#region "Handler"
    public sealed class GetCutOffsPaginationQueryHandler : IRequestHandler<GetCutOffsPaginationQuery, PaginatedList<CutOff>>
    {

        private readonly IDataContext _context;

        public GetCutOffsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }
        public async Task<PaginatedList<CutOff>> Handle(GetCutOffsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from cutoff in _context.CutOffs
                          join company in _context.Companies on cutoff.CompanyKey equals company.Key
                          where cutoff.DeletedAt == null
                          select new CutOff
                          {
                              Key = cutoff.Key,
                              CompanyKey = cutoff.CompanyKey,
                              YearPeriod = cutoff.YearPeriod,
                              Description = cutoff.Description,
                              JanStart = cutoff.JanStart,
                              JanEnd = cutoff.JanEnd,
                              FebStart = cutoff.FebStart,
                              FebEnd = cutoff.FebEnd,
                              MarStart = cutoff.MarStart,
                              MarEnd = cutoff.MarEnd,
                              AprStart = cutoff.AprStart,
                              AprEnd = cutoff.AprEnd,
                              MayStart = cutoff.MayStart,
                              MayEnd = cutoff.MayEnd,
                              JunStart = cutoff.JunStart,
                              JunEnd = cutoff.JunEnd,
                              JulStart = cutoff.JulStart,
                              JulEnd = cutoff.JulEnd,
                              AugStart = cutoff.AugStart,
                              AugEnd = cutoff.AugEnd,
                              SepStart = cutoff.SepStart,
                              SepEnd = cutoff.SepEnd,
                              OctStart = cutoff.OctStart,
                              OctEnd = cutoff.OctEnd,
                              NovStart = cutoff.NovStart,
                              NovEnd = cutoff.NovEnd,
                              DecStart = cutoff.DecStart,
                              DecEnd = cutoff.DecEnd,
                              Company = company
                          };
            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.YearPeriod.ToString(), $"%{search}%") || EF.Functions.ILike(b.Company.Name, $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var cutOffs = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

            return await Task.FromResult(cutOffs);
        }
    }
#endregion
#endregion

#region "Get Cut Off By Id"
#region "Query"
    public sealed record GetCutOffQuery(Guid Key) : IRequest<CutOffForm>;
#endregion
#region "Handler"
    public sealed class GetCutOffQueryHandler : IRequestHandler<GetCutOffQuery, CutOffForm>
    {
        private readonly IDataContext _context;
        public GetCutOffQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<CutOffForm> Handle(GetCutOffQuery request, CancellationToken cancellationToken)
        {
            var cutOff = await (from co in _context.CutOffs
                                join com in _context.Companies on co.CompanyKey equals com.Key
                                where co.Key == request.Key
                                select new CutOff
                                {
                                    Key = co.Key,
                                    CompanyKey = co.CompanyKey,
                                    YearPeriod = co.YearPeriod,
                                    Description = co.Description,
                                    JanStart = co.JanStart,
                                    JanEnd = co.JanEnd,
                                    FebStart = co.FebStart,
                                    FebEnd = co.FebEnd,
                                    MarStart = co.MarStart,
                                    MarEnd = co.MarEnd,
                                    AprStart = co.AprStart,
                                    AprEnd = co.AprEnd,
                                    MayStart = co.MayStart,
                                    MayEnd = co.MayEnd,
                                    JunStart = co.JunStart,
                                    JunEnd = co.JunEnd,
                                    JulStart = co.JulStart,
                                    JulEnd = co.JulEnd,
                                    AugStart = co.AugStart,
                                    AugEnd = co.AugEnd,
                                    SepStart = co.SepStart,
                                    SepEnd = co.SepEnd,
                                    OctStart = co.OctStart,
                                    OctEnd = co.OctEnd,
                                    NovStart = co.NovStart,
                                    NovEnd = co.NovEnd,
                                    DecStart = co.DecStart,
                                    DecEnd = co.DecEnd,
                                    Company = com
                                }).FirstOrDefaultAsync();

            if (cutOff == null)
                throw new Exception("Cut Off not found.");


            return cutOff.ConvertToViewModelCutOff();
        }
    }
#endregion
#endregion

#region "Save Cut Off"
#region "Command"
    public sealed record SaveCutOffCommand(CutOffDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveCutOffCommandHandler : IRequestHandler<SaveCutOffCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<CutOffDto> _validator;

        public SaveCutOffCommandHandler(IDataContext context, IValidator<CutOffDto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<Result> Handle(SaveCutOffCommand command, CancellationToken cancellationToken)
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

                var cutOff = command.Form.ConvertToEntity();
                if (cutOff.Key == Guid.Empty)
                {
                    cutOff.Key = Guid.NewGuid();
                }

                //Check if cutOff exists
                var existingCutOff = await _context.CutOffs
                                                   .FirstOrDefaultAsync(c => c.Key == cutOff.Key && c.DeletedAt == null, cancellationToken);
                if (existingCutOff == null)
                {
                    //Add new Cut Off
                    _context.CutOffs.Add(cutOff);
                }
                else
                {
                    //Update existing Cut Off
                    cutOff.CreatedAt = existingCutOff.CreatedAt;
                    cutOff.CreatedBy = existingCutOff.CreatedBy;
                    _context.CutOffs.Entry(existingCutOff).CurrentValues.SetValues(cutOff);
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

#region "Delete Cut Off"
#region "Command"
    public sealed record DeleteCutOffCommand(Guid Key) : IRequest<Result<CutOff>>;
#endregion
#region "Handler"
    public sealed class DeleteCutOffCommandHandler : IRequestHandler<DeleteCutOffCommand, Result<CutOff>>
    {
        private readonly IDataContext _context;

        public DeleteCutOffCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result<CutOff>> Handle(DeleteCutOffCommand command, CancellationToken cancellationToken)
        {
            var cutOff = await _context.CutOffs.FirstOrDefaultAsync(x => x.Key == command.Key);

            try
            {
                if (cutOff == null)
                    throw new Exception("Cut Off not found.");

                _context.CutOffs.Remove(cutOff);
                var result = await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<CutOff>.Failure(new[] { ex.Message });
            }

            return Result<CutOff>.Success(cutOff);
        }
    }
#endregion
#endregion
