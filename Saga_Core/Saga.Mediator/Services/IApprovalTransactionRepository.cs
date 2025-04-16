using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Programs;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Services;

public interface IApprovalTransactionRepository
{
    Task<PaginatedList<ApprovalRequestItemList>> GetApprovalRequestPaginated(string approverEmail, PaginationConfig pagination, Expression<Func<ApprovalRequest, bool>>[] wheres);
    Task<List<ApprovalStampDto>> CreateApprovalStamps(IEnumerable<ApprovalStatusDto> approvalStatuses);
}

public class ApprovalTransactionRepository : IApprovalTransactionRepository
{
    private readonly IDataContext _context;

    public ApprovalTransactionRepository(IDataContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ApprovalRequestItemList>> GetApprovalRequestPaginated(string approverEmail, PaginationConfig pagination, Expression<Func<ApprovalRequest, bool>>[] wheres)
    {
        var queries = from aps in _context.ApprovalRequests
                      where aps.ApproverEmail == approverEmail
                      select new
                      {
                          ApprovalRequest = aps
                      };

        foreach (var filter in wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x.ApprovalRequest));
        }

        if (!string.IsNullOrEmpty(pagination.Find))
        {
            string search = pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(x => EF.Functions.ILike(x.ApprovalRequest.CategoryName, $"%{search}%") ||
                                             EF.Functions.ILike(x.ApprovalRequest.SubmitterFirstName, $"%{search}%") ||
                                             EF.Functions.ILike(x.ApprovalRequest.SubmitterLastName, $"%{search}%") ||
                                             EF.Functions.ILike(x.ApprovalRequest.SubmitterEmail, $"%{search}%") ||
                                             EF.Functions.ILike(x.ApprovalRequest.Description, $"%{search}%"));
            }
        }

        var emptyApprovalStampList = Enumerable.Empty<ApprovalStampListItem>();
        var approvalStampList = (from aps in _context.ApprovalStamps
                                 join e in _context.Employees on aps.EmployeeKey equals e.DirectSupervisorKey
                                 where aps.EmployeeKey == e.DirectSupervisorKey
                                       && e.DirectSupervisorKey.HasValue && e.DeletedAt == null
                                 select new ApprovalStampListItem
                                 {
                                     Key = aps.Key,
                                     ApprovalTransactionKey = aps.ApprovalTransactionKey,
                                     EmployeeKey = aps.EmployeeKey,
                                     Level = aps.Level,
                                     Status = aps.Status,
                                     RejectReason = aps.RejectReason,
                                     DateStamp = aps.DateStamp,
                                     Approver = e
                                 }).ToList();

        var result = await queries.Select(x => new ApprovalRequestItemList
        {
            TransactionKey = x.ApprovalRequest.TransactionKey,
            SubmitterKey = x.ApprovalRequest.SubmitterKey,
            ApprovalTransactionDate = x.ApprovalRequest.ApprovalTransactionDate,
            SubmitterName = (x.ApprovalRequest.SubmitterFirstName ?? String.Empty) + " " + (x.ApprovalRequest.SubmitterLastName ?? String.Empty),
            SubmitterEmail = x.ApprovalRequest.SubmitterEmail,
            Category = x.ApprovalRequest.Category,
            CategoryName = x.ApprovalRequest.CategoryName,
            ApprovalStatus = x.ApprovalRequest.ApprovalStatus,
            StatusName = x.ApprovalRequest.StatusName,
            Description = x.ApprovalRequest.Description,
            RejectReason = x.ApprovalRequest.RejectReason,
            CurrentLevel = x.ApprovalRequest.CurrentLevel,
            MaxLevel = x.ApprovalRequest.MaxLevel,
            CanApprove = x.ApprovalRequest.CanApprove,
            CanReject = x.ApprovalRequest.CanReject,
            ApproverKey = x.ApprovalRequest.ApproverKey,
            ApproverName = (x.ApprovalRequest.ApproverFirstName ?? String.Empty) + " " + (x.ApprovalRequest.ApproverLastName ?? String.Empty),
            ApproverEmail = x.ApprovalRequest.ApproverEmail,
            ApprovalStamps = approvalStampList.Where(d => d.ApprovalTransactionKey == x.ApprovalRequest.TransactionKey).ToList() ?? emptyApprovalStampList
        }).PaginatedListAsync(pagination.PageNumber, pagination.PageSize);

        return await Task.FromResult(result);
    }

    public async Task<List<ApprovalStampDto>> CreateApprovalStamps(IEnumerable<ApprovalStatusDto> approvalStatuses)
    {
        var result = approvalStatuses
                       .Select((status, index) => new ApprovalStampDto
                       {
                           Key = Guid.NewGuid(),
                           EmployeeKey = status.ApproverKey,
                           Level = status.Level,
                           Status = status.Status,
                           DateStamp = DateTime.Now,
                           RejectReason = String.Empty
                       }).ToList();

        return await Task.FromResult(result);
    }
}
