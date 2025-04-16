using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared;
using Saga.DomainShared.Models;
using Saga.Mediator.Systems.AssetMediator;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using System.Linq.Expressions;
using Saga.Domain.Enums;
using Saga.Mediator.Services;
using Saga.Mediator.Attendances.ApprovalTransactionMediator;
using Saga.Domain.ViewModels.Systems;
using Saga.Domain.Entities.Systems;
using Microsoft.AspNetCore.Hosting;
using Saga.DomainShared.Interfaces;
using Saga.Mediator.Attendances.LeaveMediator;
using Saga.DomainShared.Extensions;

namespace Saga.Mediator.Attendances.LeaveSubmissionMediator;

#region "Get List Leave Submission"
#region "Query"
    public sealed record GetLeaveSubmissionsQuery(Expression<Func<LeaveSubmission, bool>>[] wheres) : IRequest<IEnumerable<LeaveSubmission>>;
#endregion
#region "Handler"
    public sealed class GetLeaveSubmissionsQueryHandler : IRequestHandler<GetLeaveSubmissionsQuery, IEnumerable<LeaveSubmission>>
    {
        private readonly IDataContext _context;

        public GetLeaveSubmissionsQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LeaveSubmission>> Handle(GetLeaveSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var queries = from lsb in _context.LeaveSubmissions
                          join le in _context.Leaves on lsb.LeaveKey equals le.Key
                          join emp in _context.Employees on lsb.EmployeeKey equals emp.Key
                          join com in _context.Companies on emp.CompanyKey equals com.Key
                          join apt in _context.ApprovalTransactions on lsb.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                          from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                          where lsb.DeletedAt == null
                          select new LeaveSubmission
                          {
                              Key = lsb.Key,
                              EmployeeKey = lsb.EmployeeKey,
                              LeaveKey = lsb.LeaveKey,
                              DateStart = lsb.DateStart,
                              DateEnd = lsb.DateEnd,
                              Duration = lsb.Duration,
                              ApprovalStatus = lsb.ApprovalStatus,
                              Employee = emp,
                              Leave = le,
                              Documents = lsb.Documents ?? Array.Empty<Guid>(),
                              Description = lsb.Description ?? String.Empty,
                              LeaveCode = lsb.LeaveCode,
                              Number = lsb.Number,
                              ApprovalTransactionKey = lsb.ApprovalTransactionKey,
                              ApprovalTransaction = approvalTransaction
                          };

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(filter);
            }

            var leaveSubmissions = await queries.ToListAsync();

            return leaveSubmissions;
        }
    }
#endregion
#endregion

#region "Get List Leave Submission With Pagination"
#region "Query"
    public sealed record GetLeaveSubmissionsPaginationQuery(PaginationConfig pagination, Expression<Func<LeaveSubmission, bool>>[] wheres, Guid? CompanyKey) : IRequest<PaginatedList<LeaveSubmissionListItem>>;
#endregion
#region "Handler"
    public sealed class GetLeaveSubmissionsPaginationQueryHandler : IRequestHandler<GetLeaveSubmissionsPaginationQuery, PaginatedList<LeaveSubmissionListItem>>
    {
        private readonly IDataContext _context;

        public GetLeaveSubmissionsPaginationQueryHandler(IDataContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<LeaveSubmissionListItem>> Handle(GetLeaveSubmissionsPaginationQuery request, CancellationToken cancellationToken)
        {
            var queries = from lsb in _context.LeaveSubmissions
                          join le in _context.Leaves on lsb.LeaveKey equals le.Key
                          join emp in _context.Employees on lsb.EmployeeKey equals emp.Key
                          join com in _context.Companies on emp.CompanyKey equals com.Key
                          join apt in _context.ApprovalTransactions on lsb.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                          from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                          where lsb.DeletedAt == null &&
                          (request.CompanyKey.HasValue ? emp.CompanyKey == request.CompanyKey : true)
                          select new
                          {
                              LeaveSubmission = lsb,
                              Employee = emp,
                              Leave = le,
                              Company = com,
                              ApprovalTransaction = approvalTransaction
                          };

            string search = request.pagination.Find;
            if (!string.IsNullOrEmpty(search))
            {
                queries = queries.Where(b => EF.Functions.ILike(b.LeaveSubmission.Number, $"%{search}%") || 
                                             EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") || 
                                             EF.Functions.ILike(b.Employee.LastName, $"%{search}%") || 
                                             EF.Functions.ILike(b.Company.Name, $"%{search}%") ||
                                             EF.Functions.ILike(b.LeaveSubmission.DateStart.Year.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(b.LeaveSubmission.DateStart.Month.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(b.LeaveSubmission.DateStart.Date.ToString(), $"%{search}%") ||
                                             EF.Functions.ILike(Enum.GetName(typeof(ApprovalStatus), b.LeaveSubmission.ApprovalStatus), $"%{search}%") ||
                                             EF.Functions.ILike(b.Leave.Code, $"%{search}%"));
            }

            foreach (var filter in request.wheres)
            {
                queries = queries.Where(x => filter.Compile().Invoke(x.LeaveSubmission));
            }

            var leaveSubmissions = await queries.Skip((request.pagination.PageNumber - 1) * request.pagination.PageSize)
                                    .Take(request.pagination.PageSize)
                                    .Select(x => new
                                    {
                                        x.LeaveSubmission,
                                        x.Employee,
                                        x.Leave,
                                        x.Company,
                                        x.ApprovalTransaction
                                    }).ToListAsync();

            var leaveSubmissionList = leaveSubmissions.Select(x => new LeaveSubmissionListItem
            {
                Key = x.LeaveSubmission.Key,
                EmployeeKey = x.LeaveSubmission.EmployeeKey,
                LeaveKey = x.LeaveSubmission.LeaveKey,
                DateStart = x.LeaveSubmission.DateStart,
                DateEnd = x.LeaveSubmission.DateEnd,
                Duration = x.LeaveSubmission.Duration,
                ApprovalStatus = x.LeaveSubmission.ApprovalStatus,
                StatusName = Enum.GetName(typeof(ApprovalStatus), x.LeaveSubmission.ApprovalStatus),
                Number = x.LeaveSubmission.Number,
                LeaveCategory = x.LeaveSubmission.LeaveCode,
                Employee = x.Employee,
                Leave = x.Leave,
                Company = x.Company
            }).ToList();

            return new PaginatedList<LeaveSubmissionListItem>(leaveSubmissionList, leaveSubmissions.Count(), request.pagination.PageNumber, request.pagination.PageSize);
        }
    }
#endregion
#endregion

#region "Get By Id Leave Submission"
#region "Query"
    public sealed record GetLeaveSubmissionQuery(Guid Key) : IRequest<LeaveSubmissionForm>;
#endregion
#region "Handler"
    public sealed class GetLeaveSubmissionQueryHandler : IRequestHandler<GetLeaveSubmissionQuery, LeaveSubmissionForm>
    {
        private readonly IDataContext _context;
        private readonly IEmployeeRepository _repository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GetLeaveSubmissionQueryHandler(IDataContext context, IEmployeeRepository repository, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _repository = repository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<LeaveSubmissionForm> Handle(GetLeaveSubmissionQuery request, CancellationToken cancellationToken)
        {
            var leaveSubmission = await (from lsb in _context.LeaveSubmissions
                                         join lea in _context.Leaves on lsb.LeaveKey equals lea.Key
                                         join emp in _context.Employees on lsb.EmployeeKey equals emp.Key
                                         where lsb.Key == request.Key
                                         select new LeaveSubmission
                                         {
                                             Key = lsb.Key,
                                             EmployeeKey = lsb.EmployeeKey,
                                             LeaveKey = lsb.LeaveKey,
                                             DateStart = lsb.DateStart,
                                             DateEnd = lsb.DateEnd,
                                             Duration = lsb.Duration,
                                             ApprovalStatus = lsb.ApprovalStatus,
                                             Documents = lsb.Documents,
                                             Description = lsb.Description,
                                             LeaveCode = lea.Code,
                                             Number = lsb.Number,
                                             ApprovalTransactionKey = lsb.ApprovalTransactionKey,
                                             Employee = emp,
                                             Leave = lea
                                         }).FirstOrDefaultAsync();

            if (leaveSubmission == null)
                throw new Exception("Leave Submission not found.");

            var leaveQuotas = await _repository.GetEmployeeLeaveQuotas(leaveSubmission.EmployeeKey);

            var approvalStamps = await (from apt in _context.ApprovalTransactions
                                        join ast in _context.ApprovalStamps on apt.Key equals ast.ApprovalTransactionKey
                                        join emp in _context.Employees on apt.EmployeeKey equals emp.Key
                                        join apr in _context.Approvers on ast.EmployeeKey equals apr.EmployeeKey into approverGroup
                                        from approver in approverGroup.DefaultIfEmpty()
                                        where apt.Key == leaveSubmission.ApprovalTransactionKey
                                        select new
                                        {
                                            ApprovalTransaction = apt,
                                            ApprovalStamps = ast,
                                            Approver = emp,
                                            Action = approver.Action,
                                            Level = approver.Level
                                        }).ToListAsync();

            var assets = new List<AssetForm>();
            if (leaveSubmission.Documents != null && leaveSubmission.Documents.Any())
            {
                var assetList = await _context.Assets
                                       .Where(x => leaveSubmission.Documents.Contains(x.Key))
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

            var viewModel = leaveSubmission.ConvertToLeaveSubmissionFormViewModel();
            viewModel.LeaveQuotas = leaveQuotas;

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

#region "Save Leave Submission"
#region "Command"
    public sealed record SaveLeaveSubmissionCommand(LeaveSubmissionDto Form) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class SaveLeaveSubmissionCommandHandler(IDataContext _context,
                                                          IMediator _mediator,
                                                          IValidator<LeaveSubmissionDto> _validator,
                                                          IApprovalTransactionRepository _repository) : IRequestHandler<SaveLeaveSubmissionCommand, Result>
    {
        public async Task<Result> Handle(SaveLeaveSubmissionCommand command, CancellationToken cancellationToken)
        {
            try
            {

                var leave = await _mediator.Send(new GetLeaveQuery(command.Form.LeaveKey ?? Guid.Empty), cancellationToken);

                command.Form.Leave = leave;
                command.Form.LeaveCode = leave.Code;

                //Check if approval statuses (from approval config or approval stamp) is exists
                if (command.Form.ApprovalStatuses != null && command.Form.ApprovalStatuses.Any())
                {
                    var approvalTransactionDto = new ApprovalTransactionDto
                    {
                        Key = command.Form.ApprovalTransactionKey ?? Guid.Empty,
                        EmployeeKey = command.Form.EmployeeKey ?? Guid.Empty,
                        ApprovalTransactionDate = DateTime.Now,
                        Category = ApprovalCategory.LeavePermit,
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

                var leaveSubmission = command.Form.ConvertToEntity();

                //Set documents
                leaveSubmission.Documents = documentKeys.ToArray();

                if (leaveSubmission.Key == Guid.Empty)
                {
                    leaveSubmission.Key = Guid.NewGuid();
                }

                //Check if leave submission exists
                var existingLeaveSubmission = await _context.LeaveSubmissions.FirstOrDefaultAsync(x => x.Key == leaveSubmission.Key && x.DeletedAt == null, cancellationToken);
                if (existingLeaveSubmission == null)
                {
                    //Add new Leave Submission
                    _context.LeaveSubmissions.Add(leaveSubmission); 
                } else
                {
                    //Update existing Leave Submission
                    leaveSubmission.CreatedAt = existingLeaveSubmission.CreatedAt;
                    leaveSubmission.CreatedBy = existingLeaveSubmission.CreatedBy;
                    _context.LeaveSubmissions.Entry(existingLeaveSubmission).CurrentValues.SetValues(leaveSubmission);
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

#region "Delete Leave Submission"
#region "Command"
    public sealed record DeleteLeaveSubmissionCommand(Guid Key) : IRequest<Result>;
#endregion
#region "Handler"
    public sealed class DeleteLeaveSubmissionCommandHandler : IRequestHandler<DeleteLeaveSubmissionCommand, Result>
    {
        private readonly IDataContext _context;
        private readonly IMediator _mediator;

        public DeleteLeaveSubmissionCommandHandler(IDataContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Result> Handle(DeleteLeaveSubmissionCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var leaveSubmission = await _context.LeaveSubmissions.FirstOrDefaultAsync(ls => ls.Key == command.Key, cancellationToken);
                if (leaveSubmission == null)
                    throw new Exception("Leave Submission not found.");

                //Delete approval transaction if exists
                if (leaveSubmission.ApprovalTransactionKey != Guid.Empty)
                {
                    var approvalResult = await _mediator.Send(new DeleteApprovalTransactionCommand(leaveSubmission.ApprovalTransactionKey), cancellationToken);
                    if (!approvalResult.Succeeded)
                        throw new Exception(approvalResult.Errors.FirstOrDefault());
                }

                //Get document keys to delete
                var documentKeys = leaveSubmission.Documents;

                //Delete associated assets using DeleteFileCommand
                if (documentKeys != null && documentKeys.Any())
                {
                    var deletionResults = new List<Result>();

                    foreach(var documentKey in documentKeys)
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

                _context.LeaveSubmissions.Remove(leaveSubmission);
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

#region "Get Employee Leave By Date"
#region "Query"
public sealed record GetEmployeeLeaveQuery(Guid employeeKey, DateOnly date) : IRequest<LeaveSubmission>;
#endregion
#region "Handler"
public sealed class GetEmployeeLeaveQueryHandler : IRequestHandler<GetEmployeeLeaveQuery, LeaveSubmission>
{
    private readonly IDataContext _context;

    public GetEmployeeLeaveQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<LeaveSubmission> Handle(GetEmployeeLeaveQuery request, CancellationToken cancellationToken)
    {
        var leaveSubmission = await (from e in _context.Employees
                           join lsb in _context.LeaveSubmissions on e.Key equals lsb.EmployeeKey
                           join lea in _context.Leaves on lsb.LeaveKey equals lea.Key
                           where lsb.EmployeeKey == request.employeeKey &&
                               request.date >= DateOnly.FromDateTime(lsb.DateStart) &&
                               request.date <= DateOnly.FromDateTime(lsb.DateEnd) 
                           select new LeaveSubmission
                           {
                               Key = lsb.Key,
                               EmployeeKey = lsb.EmployeeKey,
                               LeaveKey = lsb.LeaveKey,
                               DateStart = lsb.DateStart,
                               DateEnd = lsb.DateEnd,
                               Duration = lsb.Duration,
                               ApprovalStatus = lsb.ApprovalStatus,
                               Documents = lsb.Documents,
                               Description = lsb.Description,
                               LeaveCode = lea.Code,
                               Number = lsb.Number,
                               ApprovalTransactionKey = lsb.ApprovalTransactionKey,
                               Employee = e,
                               Leave = lea
                           }).FirstOrDefaultAsync();

        if (leaveSubmission == null)
            throw new Exception("Leave Submission not found.");

        return leaveSubmission;
    }
}
#endregion
#endregion

#region "Get Leave Detail Report"
#region "Query"
public sealed record GetLeaveDetailReportQuery(LeaveDetailReportDto report, PaginationConfig paginationConfig) : IRequest<LeaveDetailReport>;
#endregion
#region "Handler"
public sealed class GetLeaveDetailReportQueryHandler(IAttendanceRepository _repository) : IRequestHandler<GetLeaveDetailReportQuery, LeaveDetailReport>
{
    public async Task<LeaveDetailReport> Handle(GetLeaveDetailReportQuery request, CancellationToken cancellationToken)
    {
        var leavePermitDetails = await _repository.GetLeaveDetailReport(request.report, request.paginationConfig);

        var viewModel = new LeaveDetailReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            LeaveDetailReports = leavePermitDetails.Items,
            PageNumber = leavePermitDetails.PageNumber,
            TotalCount = leavePermitDetails.TotalCount,
            TotalPages = leavePermitDetails.TotalPages
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Leave Detail Report"
#region "Query"
public sealed record GenerateLeaveDetailReportQuery(LeaveDetailReportDto report, PaginationConfig paginationConfig) : IRequest<byte[]>;
#endregion
#region "Handler"
public sealed class GenerateLeaveDetailReportQueryHandler(IAttendanceRepository _repository,
                                                          IDocumentGenerator _documentGenerator) : IRequestHandler<GenerateLeaveDetailReportQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateLeaveDetailReportQuery request, CancellationToken cancellationToken)
    {
        var leavePermitDetails = await _repository.GetLeaveDetailReport(request.report, request.paginationConfig);

        var viewModel = new LeaveDetailReport
        {
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            LeaveDetailReports = leavePermitDetails.Items
        };

        return await _documentGenerator.GenerateAttendanceLeaveDetailReportXlsx(viewModel);
    }
}
#endregion
#endregion
