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

namespace Saga.Mediator.Organizations.BankMediator;


#region "Get List Bank"
#region "Query"
public record GetBanksQuery(Expression<Func<Bank, bool>>[] wheres) : IRequest<BankList>;
#endregion
#region "Handler"
public sealed class GetBanksQueryHandler : IRequestHandler<GetBanksQuery, BankList>
{
    private readonly IDataContext _context;

    public GetBanksQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<BankList> Handle(GetBanksQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Banks.AsQueryable().Where(b => b.DeletedAt == null);

        request.wheres.ToList()
                      .ForEach(x =>
                      {
                          queries = queries.Where(x);
                      });

        var banks = await queries.ToListAsync();

        var viewModel = new BankList
        {
            Banks = banks.Select(bank => bank.ConvertToViewModelBankListItem())
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Get List Bank With Pagination"
#region "Query"
public record GetBanksPaginationQuery(PaginationConfig pagination) : IRequest<PaginatedList<Bank>>;
#endregion
#region "Handler"
public sealed class GetBanksPaginationQueryHandler : IRequestHandler<GetBanksPaginationQuery, PaginatedList<Bank>>
{
    private readonly IDataContext _context;

    public GetBanksPaginationQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<Bank>> Handle(GetBanksPaginationQuery request, CancellationToken cancellationToken)
    {
        var queries = _context.Banks.AsQueryable().Where(b => b.DeletedAt == null);

        string search = request.pagination.Find;

        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Code, $"%{search}%") || EF.Functions.ILike(b.Name, $"%{search}%") || EF.Functions.ILike(b.Description, $"%{search}%"));
        }

        var banks = await queries.PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);

        return await Task.FromResult(banks);
    }
}
#endregion
#endregion

#region "Get By Id Bank"
#region "Query"
public record GetBankQuery(Guid Key) : IRequest<BankForm>;
#endregion
#region "Handler"
    public sealed class GetBankQueryHandler : IRequestHandler<GetBankQuery, BankForm>
    {
        private readonly IDataContext _context;

        public GetBankQueryHandler(IDataContext context)
        {
            _context = context;
        }

    public async Task<BankForm> Handle(GetBankQuery request, CancellationToken cancellationToken)
    {
        var bank = await _context.Banks.FirstOrDefaultAsync(b => b.Key == request.Key);

        if (bank == null || bank.DeletedAt != null)
        {
            throw new InvalidOperationException("Bank not found or has been deleted.");
        }

        return bank.ConvertToViewModelBankForm();
    }
}
#endregion
#endregion

#region "Save Bank"
#region "Command"
public sealed record SaveBankCommand(BankDto Form) : IRequest<Result>;
#endregion
#region "Handler"
public sealed class SaveBankCommandHandler : IRequestHandler<SaveBankCommand, Result>
{
    private readonly IDataContext _context;
    private readonly IValidator<BankDto> _validator;

    public SaveBankCommandHandler(IDataContext context, IValidator<BankDto> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<Result> Handle(SaveBankCommand command, CancellationToken cancellationToken)
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

            var bank = command.Form.ConvertToEntity();

            if (bank.Key == Guid.Empty)
            {
                bank.Key = Guid.NewGuid();
            }

            // Check if bank exist
            var existingBank = await _context.Banks
                                             .FirstOrDefaultAsync(b => b.Key == command.Form.Key && b.DeletedAt == null, cancellationToken);

            if (existingBank == null)
            {
                //Add new Bank
                _context.Banks.Add(bank);
            }
            else
            {
                //Update existing Bank
                bank.CreatedAt = existingBank.CreatedAt;
                bank.CreatedBy = existingBank.CreatedBy;
                _context.Banks.Entry(existingBank).CurrentValues.SetValues(bank);
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

#region "Delete Bank"
#region "Command"
    public sealed record DeleteBankCommand(Guid Key) : IRequest<Result<Bank>>;
#endregion
#region "Handler"
    public sealed class DeleteBankCommandHandler : IRequestHandler<DeleteBankCommand, Result<Bank>>
    {
        private readonly IDataContext _context;

        public DeleteBankCommandHandler(IDataContext context)
        {
            _context = context;
        }

    public async Task<Result<Bank>> Handle(DeleteBankCommand command, CancellationToken cancellationToken)
    {
        var bank = await _context.Banks.FirstOrDefaultAsync(e => e.Key == command.Key);

        try
        {
            if (bank == null)
            {
                throw new Exception("Bank Not Found");
            }

            _context.Banks.Remove(bank);
            var result = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<Bank>.Failure(new[] { ex.Message });
        }

        return Result<Bank>.Success(bank);
    }
}
#endregion
#endregion

