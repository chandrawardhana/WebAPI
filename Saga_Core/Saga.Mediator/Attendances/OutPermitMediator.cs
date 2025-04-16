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
using Saga.DomainShared.Models;
using Saga.Mediator.Attendances.ApprovalTransactionMediator;
using Saga.Mediator.Services;
using Saga.Mediator.Systems.AssetMediator;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;

namespace Saga.Mediator.Attendances.OutPermitMediator;

#region "Get List Out Permit"
#region "Query"
    public sealed record GetOutPermitsQuery(Expression<Func<OutPermit, bool>>[] wheres) : IRequest<IEnumerable<OutPermit>>;
#endregion
#region "Handler"
    public sealed class GetOutPermitsQueryHandler : IRequestHandler<GetOutPermitsQuery, IEnumerable<OutPermit>>
    {
        private readonly IDataContext _context;

        public GetOutPermitsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OutPermit>> Handle(GetOutPermitsQuery request, CancellationToken cancellationToken)
        {
            var queries = from op in _context.OutPermits
                          join emp in _context.Employees on op.EmployeeKey equals emp.Key
                          join com in _context.Companies on emp.CompanyKey equals com.Key
                          join apt in _context.ApprovalTransactions on op.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                          from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                          where op.DeletedAt == null && approvalTransaction.Category == ApprovalCategory.OutPermit
                          select new OutPermit
                          {
                              Key = op.Key,
                              EmployeeKey = op.EmployeeKey,
                              ApprovalTransactionKey = op.ApprovalTransactionKey,
                              Number = op.Number,
                              DateSubmission = op.DateSubmission,
                              OutPermitSubmission = op.OutPermitSubmission,
                              BackToWork = op.BackToWork,
                              Description = op.Description,
                              ApprovalStatus = op.ApprovalStatus,
                              Documents = op.Documents,
                              Employee = emp,
                              ApprovalTransaction = approvalTransaction
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(filter);
            }

            var outPermits = await queries.ToListAsync();

            return outPermits;
        }
    }
#endregion
#endregion

#region "Get List Out Permit With Pagination"
#region "Query"
    public sealed record GetOutPermitsWithPaginationQuery(PaginationConfig pagination, Expression<Func<OutPermit, bool>>[] wheres, Guid? CompanyKey) : IRequest<PaginatedList<OutPermitListItem>>;
#endregion
#region "Handler"
    public sealed class GetOutPermitsWithPaginationQueryHandler : IRequestHandler<GetOutPermitsWithPaginationQuery, PaginatedList<OutPermitListItem>>
    {
        private readonly IDataContext _context;

        public GetOutPermitsWithPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<OutPermitListItem>> Handle(GetOutPermitsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from op in _context.OutPermits
                          join emp in _context.Employees on op.EmployeeKey equals emp.Key
                          join com in _context.Companies on emp.CompanyKey equals com.Key
                          join apt in _context.ApprovalTransactions on op.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                          from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                          where op.DeletedAt == null &&
                          (request.CompanyKey.HasValue ? emp.CompanyKey == request.CompanyKey : true)
                          select new
                          {
                              OutPermit = op,
                              Employee = emp,
                              Company = com,
                              ApprovalTransaction = approvalTransaction
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.OutPermit.Number, $"%{search}%") ||
                                             EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") ||
                                             EF.Functions.ILike(b.Employee.LastName, $"%{search}%") ||
                                             EF.Functions.ILike(b.Employee.Code, $"%{search}%") ||
                                             EF.Functions.ILike(b.Company.Name, $"%{search}%") ||
                                             EF.Functions.ILike(b.OutPermit.DateSubmission.Year.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(b.OutPermit.DateSubmission.Month.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(b.OutPermit.DateSubmission.Day.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(Enum.GetName(typeof(ApprovalStatus), b.OutPermit.ApprovalStatus), $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x.OutPermit));
            }

            var outPermits = await queries.Skip((request.pagination.PageNumber - 1) * request.pagination.PageSize)
                                    .Take(request.pagination.PageSize)
                                    .Select(x => new
                                    {
                                        x.OutPermit,
                                        x.Employee,
                                        x.Company,
                                        x.ApprovalTransaction
                                    }).ToListAsync();
            var outPermitList = outPermits.Select(x => new OutPermitListItem
            {
                Key = x.OutPermit.Key,
                EmployeeKey = x.OutPermit.EmployeeKey,
                DateSubmission = x.OutPermit.DateSubmission,
                OutPermitSubmission = x.OutPermit.OutPermitSubmission,
                BackToWork = x.OutPermit.BackToWork,
                Description = x.OutPermit.Description,
                ApprovalStatus = x.OutPermit.ApprovalStatus,
                StatusName = Enum.GetName(typeof(ApprovalStatus), x.OutPermit.ApprovalStatus),
                Number = x.OutPermit.Number,
                Employee = x.Employee,
                Company = x.Company
            }).ToList();

            return new PaginatedList<OutPermitListItem>(outPermitList, outPermits.Count(), request.pagination.PageNumber, request.pagination.PageSize);
        }
    }
#endregion
#endregion

#region "Get Out Permit By Id"
#region "Query"
    public sealed record GetOutPermitQuery(Guid Key) : IRequest<OutPermitForm>;
#endregion
#region "Handler"
    public sealed class GetOutPermitQueryHandler : IRequestHandler<GetOutPermitQuery, OutPermitForm>
    {
        private readonly IDataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GetOutPermitQueryHandler(IDataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<OutPermitForm> Handle(GetOutPermitQuery request, CancellationToken cancellationToken)
        {
            var outPermit = await(from op in _context.OutPermits
                                  join emp in _context.Employees on op.EmployeeKey equals emp.Key
                                  where op.Key == request.Key
                                  select new OutPermit
                                  {
                                      Key = op.Key,
                                      EmployeeKey = op.EmployeeKey,
                                      ApprovalTransactionKey = op.ApprovalTransactionKey,
                                      Number = op.Number,
                                      DateSubmission = op.DateSubmission,
                                      OutPermitSubmission = op.OutPermitSubmission,
                                      BackToWork = op.BackToWork,
                                      Description = op.Description,
                                      ApprovalStatus = op.ApprovalStatus,
                                      Documents = op.Documents,
                                      Employee = emp
                                  }).FirstOrDefaultAsync();

            if (outPermit == null)
                throw new Exception("Out Permit not found.");

            var approvalStamps = await (from apt in _context.ApprovalTransactions
                                        join ast in _context.ApprovalStamps on apt.Key equals ast.ApprovalTransactionKey
                                        join emp in _context.Employees on apt.EmployeeKey equals emp.Key
                                        join apr in _context.Approvers on ast.EmployeeKey equals apr.EmployeeKey into approverGroup
                                        from approver in approverGroup.DefaultIfEmpty()
                                        where apt.Key == outPermit.ApprovalTransactionKey && apt.Category == ApprovalCategory.OutPermit
                                        select new
                                        {
                                            ApprovalTransaction = apt,
                                            ApprovalStamps = ast,
                                            Approver = emp,
                                            Action = approver.Action,
                                            Level = approver.Level
                                        }).ToListAsync();

            var assets = new List<AssetForm>();
            if (outPermit.Documents != null && outPermit.Documents.Any())
            {
                var assetList = await _context.Assets
                                       .Where(x => outPermit.Documents.Contains(x.Key))
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

            var viewModel = outPermit.ConvertToViewModelOutPermit();

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

#region "Save Out Permit"
#region "Command"
    public sealed record SaveOutPermitCommand(OutPermitDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveOutPermitCommandHandler(IDataContext _context,
                                                    IMediator _mediator,
                                                    IValidator<OutPermitDto> _validator,
                                                    IApprovalTransactionRepository _repository) : IRequestHandler<SaveOutPermitCommand, Result>
    {
        public async Task<Result> Handle(SaveOutPermitCommand command, CancellationToken cancellationToken)
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
                        Category = ApprovalCategory.OutPermit,
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

                var outPermit = command.Form.ConvertToEntity();

                //set documents
                outPermit.Documents = documentKeys.ToArray();

                if (outPermit.Key == Guid.Empty)
                {
                    outPermit.Key = Guid.NewGuid();
                }

                //Check if Out Permit is Exists
                var existingOutPermit = await _context.OutPermits.FirstOrDefaultAsync(x => x.Key == outPermit.Key && x.DeletedAt == null, cancellationToken);

                if (existingOutPermit == null)
                {
                    //Add new Out Permit
                    _context.OutPermits.Add(outPermit);
                }
                else
                {
                    //Update existing Out Permit
                    outPermit.CreatedAt = existingOutPermit.CreatedAt;
                    outPermit.CreatedBy = existingOutPermit.CreatedBy;
                    _context.OutPermits.Entry(existingOutPermit).CurrentValues.SetValues(outPermit);
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

#region "Delete Out Permit"
#region "Command"
    public sealed record DeleteOutPermitCommand(Guid Key) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class DeleteOutPermitCommandHandler : IRequestHandler<DeleteOutPermitCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMediator _mediator;

        public DeleteOutPermitCommandHandler(IDataContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Result> Handle(DeleteOutPermitCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var outPermit = await _context.OutPermits.FirstOrDefaultAsync(x => x.Key == command.Key, cancellationToken);

                if (outPermit == null)
                    throw new Exception("Out Permit not found.");

                //Delete approval transaction if exists
                if (outPermit.ApprovalTransactionKey != Guid.Empty)
                {
                    var approvalResult = await _mediator.Send(new DeleteApprovalTransactionCommand(outPermit.ApprovalTransactionKey), cancellationToken);
                    if (!approvalResult.Succeeded)
                        throw new Exception(approvalResult.Errors.FirstOrDefault());
                }

                //Get document keys to delete
                var documentKeys = outPermit.Documents;

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

                _context.OutPermits.Remove(outPermit);
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

#region "Get Employee Out Permit By Date"
#region "Query"
public sealed record GetEmployeeOutPermitByDateQuery(Guid employeeKey, DateOnly date) : IRequest<OutPermit>;
#endregion
#region "Handler"
public sealed class GetEmployeeOutPermitByDateQueryHandler : IRequestHandler<GetEmployeeOutPermitByDateQuery, OutPermit>
{
    private readonly IDataContext _context;

    public GetEmployeeOutPermitByDateQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<OutPermit> Handle(GetEmployeeOutPermitByDateQuery request, CancellationToken cancellationToken)
    {
        var outPermit = await (from emp in _context.Employees
                               join op in _context.OutPermits on emp.Key equals op.EmployeeKey
                               where emp.Key == request.employeeKey &&
                               op.DateSubmission == request.date
                               select new OutPermit
                               {
                                   Key = op.Key,
                                   EmployeeKey = op.EmployeeKey,
                                   ApprovalTransactionKey = op.ApprovalTransactionKey,
                                   Number = op.Number,
                                   DateSubmission = op.DateSubmission,
                                   OutPermitSubmission = op.OutPermitSubmission,
                                   BackToWork = op.BackToWork,
                                   Description = op.Description,
                                   ApprovalStatus = op.ApprovalStatus,
                                   Documents = op.Documents,
                                   Employee = emp
                               }).FirstOrDefaultAsync();

        if (outPermit == null)
            throw new Exception("Out Permit not found.");

        return outPermit;
    }
}
#endregion
#endregion
