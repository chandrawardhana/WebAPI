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

namespace Saga.Mediator.Attendances.LatePermitMediator;

#region "Get List Late Permit"
#region "Query"
    public sealed record GetLatePermitsQuery(Expression<Func<LatePermit, bool>>[] wheres) : IRequest<IEnumerable<LatePermit>>;
#endregion
#region "Handler"
    public sealed class GetLatePermitsQueryHandler : IRequestHandler<GetLatePermitsQuery, IEnumerable<LatePermit>>
    {
        private readonly IDataContext _context;

        public GetLatePermitsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LatePermit>> Handle(GetLatePermitsQuery request, CancellationToken cancellationToken)
        {
            var queries = from lp in _context.LatePermits
                          join emp in _context.Employees on lp.EmployeeKey equals emp.Key
                          join com in _context.Companies on emp.CompanyKey equals com.Key
                          join apt in _context.ApprovalTransactions on lp.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                          from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                          where lp.DeletedAt == null
                          select new LatePermit
                          {
                              Key = lp.Key,
                              EmployeeKey = lp.EmployeeKey,
                              DateSubmission = lp.DateSubmission,
                              TimeIn = lp.TimeIn,
                              Description = lp.Description,
                              ApprovalStatus = lp.ApprovalStatus,
                              Number = lp.Number,
                              Employee = emp,
                              ApprovalTransaction = approvalTransaction
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(filter);
            }

            var latePermits = await queries.ToListAsync();

            return latePermits;
        }
    }
#endregion
#endregion

#region "Get List Late Permit With Pagination"
#region "Query"
    public sealed record GetLatePermitsPaginationQuery(PaginationConfig pagination, Expression<Func<LatePermit, bool>>[] wheres, Guid? CompanyKey) : IRequest<PaginatedList<LatePermitListItem>>;
#endregion
#region "Handler"
    public sealed class GetLatePermitsPaginationQueryHandler : IRequestHandler<GetLatePermitsPaginationQuery, PaginatedList<LatePermitListItem>>
    {

        private readonly IDataContext _context;

        public GetLatePermitsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<LatePermitListItem>> Handle(GetLatePermitsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from lp in _context.LatePermits
                          join emp in _context.Employees on lp.EmployeeKey equals emp.Key
                          join com in _context.Companies on emp.CompanyKey equals com.Key
                          join apt in _context.ApprovalTransactions on lp.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                          from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                          where lp.DeletedAt == null &&
                          (request.CompanyKey.HasValue ? emp.CompanyKey == request.CompanyKey : true)
                          select new
                          {
                              LatePermit = lp,
                              Employee = emp,
                              Company = com,
                              ApprovalTransaction = approvalTransaction
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.LatePermit.Number, $"%{search}%") ||
                                             EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") || 
                                             EF.Functions.ILike(b.Employee.LastName, $"%{search}%") || 
                                             EF.Functions.ILike(b.Employee.Code, $"%{search}%") ||
                                             EF.Functions.ILike(b.Company.Name, $"%{search}%") ||
                                             EF.Functions.ILike(b.LatePermit.DateSubmission.Year.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(b.LatePermit.DateSubmission.Month.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(b.LatePermit.DateSubmission.Date.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(Enum.GetName(typeof(ApprovalStatus), b.LatePermit.ApprovalStatus), $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x.LatePermit));
            }

            var latePermits = await queries.Skip((request.pagination.PageNumber - 1) * request.pagination.PageSize)
                                    .Take(request.pagination.PageSize)
                                    .Select(x => new
                                    {
                                        x.LatePermit,
                                        x.Employee,
                                        x.Company,
                                        x.ApprovalTransaction
                                    }).ToListAsync();

            var latePermitList = latePermits.Select(x => new LatePermitListItem
            {
                Key = x.LatePermit.Key,
                EmployeeKey = x.LatePermit.EmployeeKey,
                DateSubmission = x.LatePermit.DateSubmission,
                TimeIn = x.LatePermit.TimeIn,
                Description = x.LatePermit.Description,
                ApprovalStatus = x.LatePermit.ApprovalStatus,
                StatusName = Enum.GetName(typeof(ApprovalStatus), x.LatePermit.ApprovalStatus),
                Number = x.LatePermit.Number,
                Employee = x.Employee,
                Company = x.Company
            }).ToList();

            return new PaginatedList<LatePermitListItem>(latePermitList, latePermits.Count(), request.pagination.PageNumber, request.pagination.PageSize);
        }
    }
#endregion
#endregion

#region "Get Late Permit By Id"
#region "Query"
    public sealed record GetLatePermitQuery(Guid Key) : IRequest<LatePermitForm>;
#endregion
#region "Handler"
    public sealed class GetLatePermitQueryHandler : IRequestHandler<GetLatePermitQuery, LatePermitForm>
    {
        private readonly IDataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GetLatePermitQueryHandler(IDataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<LatePermitForm> Handle(GetLatePermitQuery request, CancellationToken cancellationToken)
        {
            var latePermit = await(from lp in _context.LatePermits
                                   join emp in _context.Employees on lp.EmployeeKey equals emp.Key
                                   where lp.Key == request.Key
                                   select new LatePermit
                                   {
                                       Key = lp.Key,
                                       EmployeeKey = lp.EmployeeKey,
                                       DateSubmission = lp.DateSubmission,
                                       TimeIn = lp.TimeIn,
                                       Description = lp.Description,
                                       ApprovalStatus = lp.ApprovalStatus,
                                       Number = lp.Number,
                                       Documents = lp.Documents,
                                       ApprovalTransactionKey = lp.ApprovalTransactionKey,
                                       Employee = emp
                                   }).FirstOrDefaultAsync();

            if (latePermit == null)
                throw new Exception("Late Permit not found.");

            var approvalStamps = await (from apt in _context.ApprovalTransactions
                                    join ast in _context.ApprovalStamps on apt.Key equals ast.ApprovalTransactionKey 
                                    join emp in _context.Employees on apt.EmployeeKey equals emp.Key
                                    join apr in _context.Approvers on ast.EmployeeKey equals apr.EmployeeKey into approverGroup
                                    from approver in approverGroup.DefaultIfEmpty()
                                    where apt.Key == latePermit.ApprovalTransactionKey && apt.Category == ApprovalCategory.LatePermit
                                    select new
                                    {
                                        ApprovalTransaction = apt,
                                        ApprovalStamps = ast,
                                        Approver = emp,
                                        Action = approver.Action,
                                        Level = approver.Level
                                    }).ToListAsync();

            var assets = new List<AssetForm>();
            if (latePermit.Documents != null && latePermit.Documents.Any())
            {
                var assetList = await _context.Assets
                                       .Where(x => latePermit.Documents.Contains(x.Key))
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

            var viewModel = latePermit.ConvertToViewModelLatePermit();

            viewModel.ApprovalStatuses = approvalStamps.Select((detail, index) => new ApprovalStatusItemList
            {
                No = index + 1,
                Approver = (detail.Approver?.FirstName ?? String.Empty) + " " + (detail.Approver?.LastName ?? String.Empty),
                Action = detail.Action,
                Status = detail.ApprovalStamps.Status,
                ApprovalDate = DateOnly.FromDateTime(detail.ApprovalStamps.DateStamp),
                ApproverKey = detail.Approver?.Key,
                Level = detail.Level
            });

            viewModel.Assets = assets;

            return viewModel;
        }
    }
#endregion
#endregion

#region "Save Late Permit"
#region "Command"
    public sealed record SaveLatePermitCommand(LatePermitDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveLatePermitCommandHandler(IDataContext _context,
                                                     IMediator _mediator,
                                                     IValidator<LatePermitDto> _validator,
                                                     IApprovalTransactionRepository _repository) : IRequestHandler<SaveLatePermitCommand, Result>
    {
        public async Task<Result> Handle(SaveLatePermitCommand command, CancellationToken cancellationToken)
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
                        Category = ApprovalCategory.LatePermit,
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

                var latePermit = command.Form.ConvertToEntity();

                //Set documents
                latePermit.Documents = documentKeys.ToArray();

                if (latePermit.Key == Guid.Empty)
                {
                    latePermit.Key = Guid.NewGuid();
                }

                //Check if LatePermit Exists
                var existingLatePermit = await _context.LatePermits.FirstOrDefaultAsync(x => x.Key == latePermit.Key && x.DeletedAt == null, cancellationToken);

                if (existingLatePermit == null)
                {
                    //Add new Late Permit
                    _context.LatePermits.Add(latePermit);
                }
                else
                {
                    //Update existing Late Permit
                    latePermit.CreatedAt = existingLatePermit.CreatedAt;
                    latePermit.CreatedBy = existingLatePermit.CreatedBy;
                    _context.LatePermits.Entry(existingLatePermit).CurrentValues.SetValues(latePermit);
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

#region "Delete Late Permit"
#region "Command"
    public sealed record DeleteLatePermitCommand(Guid Key) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class DeleteLatePermitCommandHandler : IRequestHandler<DeleteLatePermitCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMediator _mediator;

        public DeleteLatePermitCommandHandler(IDataContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Result> Handle(DeleteLatePermitCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var latePermit = await _context.LatePermits.FirstOrDefaultAsync(x => x.Key == command.Key, cancellationToken);

                if (latePermit == null)
                    throw new Exception("Late Permit not found.");

                //Delete approval transaction if exists
                if (latePermit.ApprovalTransactionKey != Guid.Empty)
                {
                    var approvalResult = await _mediator.Send(new DeleteApprovalTransactionCommand(latePermit.ApprovalTransactionKey), cancellationToken);
                    if (!approvalResult.Succeeded)
                        throw new Exception(approvalResult.Errors.FirstOrDefault());
                }

                //Get document keys to delete
                var documentKeys = latePermit.Documents;

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

                _context.LatePermits.Remove(latePermit);
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

#region "Get Employee Late By Date"
#region "Query"
public sealed record GetEmployeeLateByDateQuery(Guid employeeKey, DateOnly date) : IRequest<LatePermit>;
#endregion
#region "Handler"
public sealed class GetEmployeeLateByDateQueryHandler : IRequestHandler<GetEmployeeLateByDateQuery, LatePermit>
{
    private readonly IDataContext _context;

    public GetEmployeeLateByDateQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<LatePermit> Handle(GetEmployeeLateByDateQuery request, CancellationToken cancellationToken)
    {
        var latePermit = await (from emp in _context.Employees
                                join lp in _context.LatePermits on emp.Key equals lp.EmployeeKey
                                where emp.Key == request.employeeKey &&
                                DateOnly.FromDateTime(lp.DateSubmission) == request.date
                                select new LatePermit
                                {
                                    Key = lp.Key,
                                    EmployeeKey = lp.EmployeeKey,
                                    DateSubmission = lp.DateSubmission,
                                    TimeIn = lp.TimeIn,
                                    Description = lp.Description,
                                    ApprovalStatus = lp.ApprovalStatus,
                                    Number = lp.Number,
                                    Documents = lp.Documents,
                                    ApprovalTransactionKey = lp.ApprovalTransactionKey,
                                    Employee = emp
                                }).FirstOrDefaultAsync();

        if (latePermit == null)
            throw new Exception("Late Permit not found.");

        return latePermit;
    }
}
#endregion
#endregion

#region "Get Attendance Late Permit Detail Report"
#region "Query"
public sealed record GetAttendanceLateDetailReportQuery(LateDetailReportDto report, PaginationConfig paginationConfig) : IRequest<LateDetailReport>;
#endregion
#region "Handler"
public sealed class GetAttendanceLateDetailReportQueryHandler(IAttendanceRepository _repository) : IRequestHandler<GetAttendanceLateDetailReportQuery, LateDetailReport>
{
    public async Task<LateDetailReport> Handle(GetAttendanceLateDetailReportQuery request, CancellationToken cancellationToken)
    {
        var latePermitDetails = await _repository.GetLateDetailReport(request.report, request.paginationConfig);

        var viewModel = new LateDetailReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            LatePermitReportDetails = latePermitDetails.Items,
            PageNumber = latePermitDetails.PageNumber,
            TotalCount = latePermitDetails.TotalCount,
            TotalPages = latePermitDetails.TotalPages
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Attendance Late Permit Detail Report"
#region "Query"
public sealed record GenerateAttendanceLateDetailReportQuery(LateDetailReportDto report, PaginationConfig paginationConfig) : IRequest<byte[]>;
#endregion
#region "Handler"
public sealed class GenerateAttendanceLateDetailReportQueryHandler(IAttendanceRepository _repository,
                                                                   IDocumentGenerator _documentGenerator) : IRequestHandler<GenerateAttendanceLateDetailReportQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateAttendanceLateDetailReportQuery request, CancellationToken cancellationToken)
    {
        var latePermitDetails = await _repository.GetLateDetailReport(request.report, request.paginationConfig);

        var viewModel = new LateDetailReport
        {
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            LatePermitReportDetails = latePermitDetails.Items
        };

        return await _documentGenerator.GenerateAttendanceLateDetailReportXlsx(viewModel);
    }
}
#endregion
#endregion
