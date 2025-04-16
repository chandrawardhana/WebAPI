using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Enums;
using Saga.Domain.ViewModels.Attendances;
using Saga.Domain.ViewModels.Systems;
using Saga.DomainShared;
using Saga.DomainShared.Extensions;
using Saga.DomainShared.Interfaces;
using Saga.DomainShared.Models;
using Saga.Mediator.Attendances.ApprovalTransactionMediator;
using Saga.Mediator.Services;
using Saga.Mediator.Systems.AssetMediator;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.EarlyOutPermitMediator;

#region "Get List Early Out Permit"
#region "Query"
    public sealed record GetEarlyOutPermitsQuery(Expression<Func<EarlyOutPermit, bool>>[] wheres) : IRequest<IEnumerable<EarlyOutPermit>>;
#endregion
#region "Handler"
    public sealed class GetEarlyOutPermitsQueryHandler : IRequestHandler<GetEarlyOutPermitsQuery, IEnumerable<EarlyOutPermit>>
    {
        private readonly IDataContext _context;

        public GetEarlyOutPermitsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EarlyOutPermit>> Handle(GetEarlyOutPermitsQuery request, CancellationToken cancellationToken)
        {
            var queries = from eop in _context.EarlyOutPermits
                          join emp in _context.Employees on eop.EmployeeKey equals emp.Key
                          join com in _context.Companies on emp.CompanyKey equals com.Key
                          join apt in _context.ApprovalTransactions on eop.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                          from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                          where eop.DeletedAt == null && approvalTransaction.Category == ApprovalCategory.EarlyOutPermit
                          select new EarlyOutPermit
                          {
                              Key = eop.Key,
                              EmployeeKey = eop.EmployeeKey,
                              DateSubmission = eop.DateSubmission,
                              TimeOut = eop.TimeOut,
                              Description = eop.Description,
                              ApprovalStatus = eop.ApprovalStatus,
                              Number = eop.Number,
                              Employee = emp
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(filter);
            }

            var earlyOutPermits = await queries.ToListAsync();
        
            return earlyOutPermits;
        }
    }
#endregion
#endregion

#region "Get List Early Out Permit With Pagination"
#region "Query"
    public sealed record GetEarlyOutPermitsPaginationQuery(PaginationConfig pagination, Expression<Func<EarlyOutPermit, bool>>[] wheres, Guid? CompanyKey) : IRequest<PaginatedList<EarlyOutPermitListItem>>;
#endregion
#region "Handler"
    public sealed class GetEarlyOutPermitsPaginationQueryHandler : IRequestHandler<GetEarlyOutPermitsPaginationQuery, PaginatedList<EarlyOutPermitListItem>>
    {
        private readonly IDataContext _context;

        public GetEarlyOutPermitsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<EarlyOutPermitListItem>> Handle(GetEarlyOutPermitsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from eop in _context.EarlyOutPermits
                          join emp in _context.Employees on eop.EmployeeKey equals emp.Key
                          join com in _context.Companies on emp.CompanyKey equals com.Key
                          join apt in _context.ApprovalTransactions on eop.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                          from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                          where eop.DeletedAt == null &&
                          (request.CompanyKey.HasValue ? emp.CompanyKey == request.CompanyKey : true)
                          select new
                          {
                              EarlyOutPermit = eop,
                              Employee = emp,
                              Company = com,
                              ApprovalTransaction = approvalTransaction
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.EarlyOutPermit.Number, $"%{search}%") ||
                                             EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") ||
                                             EF.Functions.ILike(b.Employee.LastName, $"%{search}%") ||
                                             EF.Functions.ILike(b.Employee.Code, $"%{search}%") ||
                                             EF.Functions.ILike(b.Company.Name, $"%{search}%") ||
                                             EF.Functions.ILike(b.EarlyOutPermit.DateSubmission.Year.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(b.EarlyOutPermit.DateSubmission.Month.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(b.EarlyOutPermit.DateSubmission.Day.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(Enum.GetName(typeof(ApprovalStatus), b.EarlyOutPermit.ApprovalStatus), $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x.EarlyOutPermit));
            }

            var earlyOutPermits = await queries.Skip((request.pagination.PageNumber - 1) * request.pagination.PageSize)
                                    .Take(request.pagination.PageSize)
                                    .Select(x => new
                                    {
                                        x.EarlyOutPermit,
                                        x.Employee,
                                        x.Company,
                                        x.ApprovalTransaction
                                    }).ToListAsync();

            var earlyOutPermitList = earlyOutPermits.Select(x => new EarlyOutPermitListItem
            {
                Key = x.EarlyOutPermit.Key,
                EmployeeKey = x.EarlyOutPermit.EmployeeKey,
                DateSubmission = x.EarlyOutPermit.DateSubmission,
                TimeOut = x.EarlyOutPermit.TimeOut,
                Description = x.EarlyOutPermit.Description,
                ApprovalStatus = x.EarlyOutPermit.ApprovalStatus,
                StatusName = Enum.GetName(typeof(ApprovalStatus), x.EarlyOutPermit.ApprovalStatus),
                Number = x.EarlyOutPermit.Number,
                Employee = x.Employee,
                Company = x.Company
            }).ToList();

            return new PaginatedList<EarlyOutPermitListItem>(earlyOutPermitList, earlyOutPermits.Count(), request.pagination.PageNumber, request.pagination.PageSize);
        }
    }
#endregion
#endregion

#region "Get Early Out Permit By Id"
#region "Query"
    public sealed record GetEarlyOutPermitQuery(Guid Key) : IRequest<EarlyOutPermitForm>;
#endregion
#region "Handler"
    public sealed class GetEarlyOutPermitQueryHandler : IRequestHandler<GetEarlyOutPermitQuery, EarlyOutPermitForm>
    {
        private readonly IDataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GetEarlyOutPermitQueryHandler(IDataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<EarlyOutPermitForm> Handle(GetEarlyOutPermitQuery request, CancellationToken cancellationToken)
        {
            var earlyOutPermit = await(from eop in _context.EarlyOutPermits
                                       join emp in _context.Employees on eop.EmployeeKey equals emp.Key
                                       where eop.Key == request.Key
                                       select new EarlyOutPermit
                                       {
                                           Key = eop.Key,
                                           EmployeeKey = eop.EmployeeKey,
                                           Number = eop.Number,
                                           DateSubmission = eop.DateSubmission,
                                           TimeOut = eop.TimeOut,
                                           Description = eop.Description,
                                           ApprovalStatus = eop.ApprovalStatus,
                                           ApprovalTransactionKey = eop.ApprovalTransactionKey,
                                           Documents = eop.Documents,
                                           Employee = emp
                                       }).FirstOrDefaultAsync();

            if (earlyOutPermit == null)
                throw new Exception("Early Out Permit not found.");

            var approvalStamps = await (from apt in _context.ApprovalTransactions
                                        join ast in _context.ApprovalStamps on apt.Key equals ast.ApprovalTransactionKey
                                        join emp in _context.Employees on apt.EmployeeKey equals emp.Key
                                        join apr in _context.Approvers on ast.EmployeeKey equals apr.EmployeeKey into approverGroup
                                        from approver in approverGroup.DefaultIfEmpty()
                                        where apt.Key == earlyOutPermit.ApprovalTransactionKey && apt.Category == ApprovalCategory.EarlyOutPermit
                                        select new
                                        {
                                            ApprovalTransaction = apt,
                                            ApprovalStamps = ast,
                                            Approver = emp,
                                            Action = approver.Action,
                                            Level = approver.Level
                                        }).ToListAsync();

            var assets = new List<AssetForm>();
            if (earlyOutPermit.Documents != null && earlyOutPermit.Documents.Any())
            {
                var assetList = await _context.Assets
                                       .Where(x => earlyOutPermit.Documents.Contains(x.Key))
                                       .ToListAsync();

                foreach (var asset in assetList)
                {
                    var assetForm = new AssetForm
                    {
                        Key = asset.Key,
                        FileName = asset.FileName,
                        OriginalFileName = asset.OriginalFileName,
                        MimeType = asset.MimeType,
                        UploadAt = asset.UploadAt
                    };

                    var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources/Uploads", asset.FileName);
                    if (File.Exists(filePath))
                    {
                        assetForm.FilePath = filePath;
                        assetForm.FileData = await File.ReadAllBytesAsync(filePath);
                    }

                    assets.Add(assetForm);
                }
            }

            var viewModel = earlyOutPermit.ConvertToViewModelEarlyOutPermit();

            viewModel.ApprovalStatuses = approvalStamps.Select((detail, index) => new ApprovalStatusItemList
            {
                No = index + 1,
                Approver = (detail.Approver?.FirstName ?? String.Empty) + " " + (detail.Approver?.LastName ?? String.Empty),
                Action = detail.Action,
                Status = detail.ApprovalStamps.Status,
                ApprovalDate = DateOnly.FromDateTime(detail.ApprovalStamps.DateStamp),
                ApproverKey = detail.Approver?.Key,
                Level = detail.Level
            }).ToList();

            viewModel.Assets = assets;

            return viewModel;
        }
    }
#endregion
#endregion

#region "Save Early Out Permit"
#region "Command"
    public sealed record SaveEarlyOutPermitCommand(EarlyOutPermitDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveEarlyOutPermitCommandHandler(IDataContext _context,
                                                         IMediator _mediator,
                                                         IValidator<EarlyOutPermitDto> _validator,
                                                         IApprovalTransactionRepository _repository) : IRequestHandler<SaveEarlyOutPermitCommand, Result>
    {
        public async Task<Result> Handle(SaveEarlyOutPermitCommand command, CancellationToken cancellationToken)
        {
            try
            {
                //Check if approval statuses (from approval config or approval stamp) is exists
                if (command.Form.ApprovalStatuses != null && command.Form.ApprovalStatuses.Any())
                {
                    var approvalTransactionDto = new ApprovalTransactionDto
                    {
                        Key = command.Form.ApprovalTransactionKey ?? Guid.Empty,
                        EmployeeKey = command.Form.EmployeeKey ?? Guid.Empty,
                        ApprovalTransactionDate = DateTime.Now,
                        Category = ApprovalCategory.EarlyOutPermit,
                        ApprovalStatus = ApprovalStatus.New,
                        RejectReason = String.Empty,
                        Description = command.Form.Description ?? String.Empty,
                        ApprovalStamps = await _repository.CreateApprovalStamps(command.Form.ApprovalStatuses)
                    };

                    //Save approval transaction
                    var approvalResult = await _mediator.Send(new SaveApprovalTransactionCommand(approvalTransactionDto), cancellationToken);
                    if (!approvalResult.Succeeded)
                        return approvalResult;

                    command.Form.ApprovalTransactionKey = approvalTransactionDto.Key;
                }

                ValidationResult validator = await _validator.ValidateAsync(command.Form);
                if (!validator.IsValid)
                {
                    var failures = validator.Errors
                                            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}")
                                            .ToList();
                    return Result.Failure(failures);
                }

                //Upload documents if any
                var documentKeys = new List<Guid>();

                if (command.Form.DocumentFiles != null && command.Form.DocumentFiles.Any())
                {
                    foreach (var file in command.Form.DocumentFiles)
                    {
                        var uploadResult = await _mediator.Send(new UploadFileCommand(file), cancellationToken);

                        if (uploadResult.Succeeded && uploadResult.Value != null)
                        {
                            documentKeys.Add(uploadResult.Value.Key);
                        }
                    }
                }

                if (command.Form.ExistingDocuments != null)
                {
                    documentKeys.AddRange(command.Form.ExistingDocuments.Where(d => d != Guid.Empty));
                }

                var earlyOutPermit = command.Form.ConvertToEntity();

                //set documents
                earlyOutPermit.Documents = documentKeys.ToArray();

                if (earlyOutPermit.Key == Guid.Empty)
                {
                    earlyOutPermit.Key = Guid.NewGuid();
                }

                //Check if Early Out Permit Exists
                var existingEarlyOutPermit = await _context.EarlyOutPermits.FirstOrDefaultAsync(x => x.Key == earlyOutPermit.Key && x.DeletedAt == null, cancellationToken);

                if (existingEarlyOutPermit == null)
                {
                    //Add new Early Out Permit
                    _context.EarlyOutPermits.Add(earlyOutPermit);
                }
                else 
                {
                    //Update existing Early Out Permit
                    earlyOutPermit.CreatedAt = existingEarlyOutPermit.CreatedAt;
                    earlyOutPermit.CreatedBy = existingEarlyOutPermit.CreatedBy;
                    _context.EarlyOutPermits.Entry(existingEarlyOutPermit).CurrentValues.SetValues(earlyOutPermit);
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

#region "Delete Early Out Permit"
#region "Command"
    public sealed record DeleteEarlyOutPermitCommand(Guid Key) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class DeleteEarlyOutPermitCommandHandler : IRequestHandler<DeleteEarlyOutPermitCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMediator _mediator;

        public DeleteEarlyOutPermitCommandHandler(IDataContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Result> Handle(DeleteEarlyOutPermitCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var earlyOutPermit = await _context.EarlyOutPermits.FirstOrDefaultAsync(x => x.Key == command.Key, cancellationToken);

                if (earlyOutPermit == null)
                    throw new Exception("Early Out Permit not found.");

                //Delete approval transaction if exists
                if (earlyOutPermit.ApprovalTransactionKey != Guid.Empty)
                {
                    var approvalResult = await _mediator.Send(new DeleteApprovalTransactionCommand(earlyOutPermit.ApprovalTransactionKey), cancellationToken);
                    if (!approvalResult.Succeeded)
                        throw new Exception(approvalResult.Errors.FirstOrDefault());
                }

                //Get document keys to delete
                var documentKeys = earlyOutPermit.Documents;

                //Delete associated assets using DeleteFileCommand
                if (documentKeys != null && documentKeys.Any())
                {
                    var deletionResults = new List<Result>();

                    foreach (var documentKey in documentKeys)
                    {
                        //Call DeleteFileCommand for each document
                        var deleteCommand = new DeleteFileCommand(documentKey);
                        var deleteResult = await _mediator.Send(deleteCommand, cancellationToken);

                        deletionResults.Add(deleteResult);
                    }

                    // Check if any deletions failed
                    var failedDeletions = deletionResults.Where(r => !r.Succeeded).ToList();
                    if (failedDeletions.Any())
                    {
                        // Collect error messages from failed deletions
                        var errorMessages = failedDeletions
                            .SelectMany(r => r.Errors)
                            .ToArray();

                        return Result.Failure(errorMessages);
                    }
                }

                _context.EarlyOutPermits.Remove(earlyOutPermit);
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

#region "Get Employee EarlyOut Permit By Date"
#region "Query"
public sealed record GetEmployeeEarlyOutByDateQuery(Guid employeeKey, DateOnly date) : IRequest<EarlyOutPermit>;
#endregion
#region "Handler"
public sealed class GetEmployeeEarlyOutByDateQueryHandler : IRequestHandler<GetEmployeeEarlyOutByDateQuery, EarlyOutPermit>
{
    private readonly IDataContext _context;

    public GetEmployeeEarlyOutByDateQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<EarlyOutPermit> Handle(GetEmployeeEarlyOutByDateQuery request, CancellationToken cancellationToken)
    {
        var earlyOutPermit = await (from emp in _context.Employees
                                    join eop in _context.EarlyOutPermits on emp.Key equals eop.EmployeeKey
                                    where emp.Key == request.employeeKey &&
                                    eop.DateSubmission == request.date
                                    select new EarlyOutPermit
                                    {
                                        Key = eop.Key,
                                        EmployeeKey = eop.EmployeeKey,
                                        Number = eop.Number,
                                        DateSubmission = eop.DateSubmission,
                                        TimeOut = eop.TimeOut,
                                        Description = eop.Description,
                                        ApprovalStatus = eop.ApprovalStatus,
                                        ApprovalTransactionKey = eop.ApprovalTransactionKey,
                                        Documents = eop.Documents,
                                        Employee = emp
                                    }).FirstOrDefaultAsync();

        if (earlyOutPermit == null)
            throw new Exception("Early Out Permit not found.");

        return earlyOutPermit;
    }
}
#endregion
#endregion