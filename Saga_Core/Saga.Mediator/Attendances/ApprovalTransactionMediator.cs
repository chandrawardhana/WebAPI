using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Programs;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.DomainShared.Interfaces;
using Saga.DomainShared.Models;
using Saga.Mediator.Services;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.ApprovalTransactionMediator;

#region "Get List Approval Transaction"
#region "Query"
    public sealed record GetApprovalTransactionsQuery(Expression<Func<ApprovalTransaction, bool>>[] wheres) : IRequest<IEnumerable<ApprovalTransactionListItem>>;
#endregion
#region "Handler"
    public sealed class GetApprovalTransactionsQueryHandler : IRequestHandler<GetApprovalTransactionsQuery, IEnumerable<ApprovalTransactionListItem>>
    {
        private readonly IDataContext _context; 

        public GetApprovalTransactionsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApprovalTransactionListItem>> Handle(GetApprovalTransactionsQuery request, CancellationToken cancellationToken)
        {
            var queries = from apt in _context.ApprovalTransactions
                          join e in _context.Employees on apt.EmployeeKey equals e.Key
                          where apt.DeletedAt == null
                          select new ApprovalTransaction
                          {
                              Key = apt.Key,
                              EmployeeKey = apt.EmployeeKey,
                              ApprovalTransactionDate = apt.ApprovalTransactionDate,
                              Category = apt.Category,
                              ApprovalStatus = apt.ApprovalStatus,
                              RejectReason = apt.RejectReason,
                              Description = apt.Description,
                              Submitter = e
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x));
            }

            var approvalTransactions = await queries.ToListAsync();

            var viewModel = approvalTransactions.Select(x => x.ConvertToViewModelApprovalTransactionListItem()).ToList();

            return viewModel;
        }
    }
#endregion
#endregion

#region "Get List Approval Transaction With Pagination"
#region "Query"
    public sealed record GetApprovalTransactionsPaginationQuery(PaginationConfig pagination, Expression<Func<ApprovalRequest, bool>>[] wheres) : IRequest<PaginatedList<ApprovalRequestItemList>>;
#endregion
#region "Handler"
    public sealed class GetApprovalTransactionsPaginationQueryHandler : IRequestHandler<GetApprovalTransactionsPaginationQuery, PaginatedList<ApprovalRequestItemList>>
    {
        private readonly IApprovalTransactionRepository _repository;
        private readonly ICurrentUser _currentUser;

        public GetApprovalTransactionsPaginationQueryHandler(IApprovalTransactionRepository repository, 
                                                             ICurrentUser currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<PaginatedList<ApprovalRequestItemList>> Handle(GetApprovalTransactionsPaginationQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated");

            var approvalRequests = await _repository.GetApprovalRequestPaginated(userId, request.pagination, request.wheres);

            return approvalRequests;
        }
    }
#endregion
#endregion

#region "Get By Id Approval Transaction"
#region "Query"
    public sealed record GetApprovalTransactionQuery(Guid Key) : IRequest<ApprovalTransactionForm>;
#endregion
#region "Handler"
    public sealed class GetApprovalTransactionQueryHandler : IRequestHandler<GetApprovalTransactionQuery, ApprovalTransactionForm>
    {
        private readonly IDataContext _context;

        public GetApprovalTransactionQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<ApprovalTransactionForm> Handle(GetApprovalTransactionQuery request, CancellationToken cancellationToken)
        {
            var approvalTransaction = await (from apt in _context.ApprovalTransactions
                                             join e in _context.Employees on apt.Key equals e.Key
                                             where apt.Key == request.Key
                                             select new ApprovalTransaction
                                             {
                                                 Key = apt.Key,
                                                 ApprovalTransactionDate = apt.ApprovalTransactionDate,
                                                 Category = apt.Category,
                                                 ApprovalStatus = apt.ApprovalStatus,
                                                 RejectReason = apt.RejectReason,
                                                 Description = apt.Description,
                                                 Submitter = e
                                             }).FirstOrDefaultAsync();

            if (approvalTransaction == null)
                throw new Exception("Approval Transaction Not Found");

            var stamps = await (from apd in _context.ApprovalStamps
                                 join emp in _context.Employees on apd.EmployeeKey equals emp.Key
                                 where apd.ApprovalTransactionKey == request.Key && apd.DeletedAt == null
                                 select new ApprovalStamp
                                 {
                                     ApprovalTransactionKey = apd.ApprovalTransactionKey,
                                     EmployeeKey = apd.EmployeeKey,
                                     Level = apd.Level,
                                     Status = apd.Status,
                                     RejectReason = apd.RejectReason,
                                     DateStamp = apd.DateStamp,
                                     Approver = emp
                                 }).ToListAsync();

            var stampsForm = stamps.Select(d => d.ConvertToViewModelApprovalStamp());

            approvalTransaction.ApprovalStamps = stamps;

            var viewModel = approvalTransaction.ConvertToViewModelApprovalTransactionForm();
            viewModel.JsonApprovalStamps = JsonConvert.SerializeObject(stampsForm, Formatting.None);

            return viewModel;
        }
    }
#endregion
#endregion

#region "Save Approval Transaction"
#region "Command"
    public sealed record SaveApprovalTransactionCommand(ApprovalTransactionDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveApprovalTransactionCommandHandler : IRequestHandler<SaveApprovalTransactionCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IValidator<ApprovalTransactionDto> _validator;
        private readonly IValidator<ApprovalStampDto> _stampValidator;

        public SaveApprovalTransactionCommandHandler(IDataContext context, IValidator<ApprovalTransactionDto> validator, IValidator<ApprovalStampDto> stampValidator)
        {
            _context = context;
            _validator = validator;
            _stampValidator = stampValidator;
        }

        public async Task<Result> Handle(SaveApprovalTransactionCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validate Approval Transaction
                ValidationResult validator = await _validator.ValidateAsync(command.Form);
                if (!validator.IsValid)
                {
                    var failures = validator.Errors
                                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                            .ToList();
                    return Result.Failure(failures);
                }

                var approvalTransaction = command.Form.ConvertToEntity();


                if (approvalTransaction.Key == Guid.Empty)
                {
                    approvalTransaction.Key = Guid.NewGuid();
                    approvalTransaction.ApprovalStatus = ApprovalStatus.Waiting;
                }

                //Check if approval transaction is exists
                var existingApprovalTransaction = await _context.ApprovalTransactions.FirstOrDefaultAsync(x => x.Key == approvalTransaction.Key, cancellationToken);

                if (existingApprovalTransaction == null)
                {
                    //Add new approval transaction
                    _context.ApprovalTransactions.Add(approvalTransaction);
                }
                else
                {
                    existingApprovalTransaction.Category = approvalTransaction.Category;
                    existingApprovalTransaction.ApprovalStatus = DetermineTransactionStatus(approvalTransaction.ApprovalStamps ?? Enumerable.Empty<ApprovalStamp>());
                    existingApprovalTransaction.RejectReason = approvalTransaction.RejectReason;
                    _context.ApprovalTransactions.Update(existingApprovalTransaction);

                    await UpdateSourceApprovalStatus(existingApprovalTransaction, cancellationToken);
                }

                if (command.Form.ApprovalStamps != null && command.Form.ApprovalStamps.Any())
                {
                    foreach (var stampDto in command.Form.ApprovalStamps)
                    {
                        stampDto.ApprovalTransactionKey = approvalTransaction.Key;
                        ValidationResult stampValidator = await _stampValidator.ValidateAsync(stampDto);
                        if (!stampValidator.IsValid)
                            return Result.Failure(stampValidator.Errors.Select(x => $"{x.PropertyName}: {x.ErrorMessage}").ToList());
                    }

                    _ = await _context.ApprovalStamps
                              .Where(x => x.ApprovalTransactionKey == approvalTransaction.Key)
                              .ExecuteDeleteAsync();

                    var stampEntities = command.Form.ApprovalStamps?.Select(x => x.ConvertToEntity());

                    foreach (var detail in approvalTransaction.ApprovalStamps ?? Enumerable.Empty<ApprovalStamp>())
                    {
                        detail.Key = Guid.NewGuid();
                        detail.ApprovalTransactionKey = approvalTransaction.Key;
                        
                        _context.ApprovalStamps.Add(detail);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { ex.Message });
            }
            return Result.Success();
        }

        private async Task UpdateSourceApprovalStatus(ApprovalTransaction approvalTransaction, CancellationToken cancellationToken)
        {
            switch (approvalTransaction.Category)
            {
                case ApprovalCategory.LeavePermit:
                    var leaveSubmission = await _context.LeaveSubmissions.FirstOrDefaultAsync(x => x.ApprovalTransactionKey == approvalTransaction.Key);
                    if (leaveSubmission != null)
                    {
                        leaveSubmission.ApprovalStatus = approvalTransaction.ApprovalStatus;
                        _context.LeaveSubmissions.Update(leaveSubmission);
                    }
                break;
                case ApprovalCategory.LatePermit:
                    var latePermit = await _context.LatePermits.FirstOrDefaultAsync(x => x.ApprovalTransactionKey == approvalTransaction.Key);
                    if (latePermit != null)
                    {
                        latePermit.ApprovalStatus = approvalTransaction.ApprovalStatus;
                        _context.LatePermits.Update(latePermit);
                    }
                break;
                case ApprovalCategory.EarlyOutPermit:
                    var earlyOutPermit = await _context.EarlyOutPermits.FirstOrDefaultAsync(x => x.ApprovalTransactionKey == approvalTransaction.Key);
                    if (earlyOutPermit != null)
                    {
                        earlyOutPermit.ApprovalStatus = approvalTransaction.ApprovalStatus;
                        _context.EarlyOutPermits.Update(earlyOutPermit);
                    }
                break;
                case ApprovalCategory.OutPermit:
                    var outPermit = await _context.OutPermits.FirstOrDefaultAsync(x => x.ApprovalTransactionKey == approvalTransaction.Key);
                    if (outPermit != null)
                    {
                        outPermit.ApprovalStatus = approvalTransaction.ApprovalStatus;
                        _context.OutPermits.Update(outPermit);
                    }
                break;
                case ApprovalCategory.OvertimeLetter:
                    var overtimeLetter = await _context.OvertimeLetters.FirstOrDefaultAsync(x => x.ApprovalTransactionKey == approvalTransaction.Key);
                    if (overtimeLetter != null)
                    {
                        overtimeLetter.ApprovalStatus = approvalTransaction.ApprovalStatus;
                        _context.OvertimeLetters.Update(overtimeLetter);
                    }
                break;
            }
        }

        private ApprovalStatus DetermineTransactionStatus(IEnumerable<ApprovalStamp> stamps)
        {
            if (!stamps.Any())
                return ApprovalStatus.Waiting;

            // If any stamp is rejected, the whole transaction is rejected
            if (stamps.Any(d => d.Status == ApprovalStatus.Reject))
                return ApprovalStatus.Reject;

            // If any stamp requires revision, the whole transaction needs revision
            if (stamps.Any(d => d.Status == ApprovalStatus.Revision))
                return ApprovalStatus.Revision;

            // If all stamps are approved
            if (stamps.All(d => d.Status == ApprovalStatus.Approve))
                return ApprovalStatus.Approve;

            // If at least one stamps is approved but not all
            if (stamps.Any(d => d.Status == ApprovalStatus.Approve))
                return ApprovalStatus.Waiting;

            return ApprovalStatus.Waiting;
        }
    }
#endregion
#endregion

#region "Delete Approval Transaction"
#region "Command"
    public sealed record DeleteApprovalTransactionCommand(Guid Key) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class DeleteApprovalTransactionCommandHandler : IRequestHandler<DeleteApprovalTransactionCommand, Result>
    {
        private readonly IDataContext _context;

        public DeleteApprovalTransactionCommandHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteApprovalTransactionCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var approvalTransaction = await _context.ApprovalTransactions.FirstOrDefaultAsync(x => x.Key == command.Key);
                if (approvalTransaction == null)
                    throw new Exception("Approval Transaction not found.");

                //Check if any approval transaction details exist
                var approvalStamps = await _context.ApprovalStamps.Where(sd => sd.ApprovalTransactionKey == approvalTransaction.Key).ToListAsync();
                if (approvalStamps.Any())
                {
                    _context.ApprovalStamps.RemoveRange(approvalStamps);
                }

                _context.ApprovalTransactions.Remove(approvalTransaction);
                await _context.SaveChangesAsync(cancellationToken);
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
