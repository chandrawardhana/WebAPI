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

namespace Saga.Mediator.Attendances.OvertimeLetterMediator;

#region "Get List Overtime Letter"
#region "Query"
public sealed record GetOvertimeLettersQuery(Expression<Func<OvertimeLetter, bool>>[] wheres) : IRequest<IEnumerable<OvertimeLetter>>;
#endregion
#region "Handler"
public sealed class GetOvertimeLettersQueryHandler : IRequestHandler<GetOvertimeLettersQuery, IEnumerable<OvertimeLetter>>
{
    private readonly IDataContext _context;

    public GetOvertimeLettersQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OvertimeLetter>> Handle(GetOvertimeLettersQuery request, CancellationToken cancellationToken)
    {
        var queries = from otl in _context.OvertimeLetters
                      join emp in _context.Employees on otl.EmployeeKey equals emp.Key
                      join com in _context.Companies on emp.CompanyKey equals com.Key
                      join apt in _context.ApprovalTransactions on otl.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                      from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                      where otl.DeletedAt == null && approvalTransaction.Category == ApprovalCategory.OvertimeLetter 
                      select new OvertimeLetter
                      {
                          Key = otl.Key,
                          EmployeeKey = otl.EmployeeKey,
                          DateSubmission = otl.DateSubmission,
                          OvertimeIn = otl.OvertimeIn,
                          OvertimeOut = otl.OvertimeOut,
                          Description = otl.Description,
                          ApprovalStatus = otl.ApprovalStatus,
                          Number = otl.Number,
                          Employee = emp,
                          ApprovalTransaction = approvalTransaction
                      };

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(filter);
        }

        var overtimeLetters = await queries.ToListAsync();

        return overtimeLetters;
    }
}
#endregion
#endregion

#region "Get List Overtime Letter With Pagination"
#region "Query"
public sealed record GetOvertimeLettersWithPaginationQuery(PaginationConfig pagination, Expression<Func<OvertimeLetter, bool>>[] wheres, Guid? CompanyKey) : IRequest<PaginatedList<OvertimeLetterListItem>>;
#endregion
#region "Handler"
public sealed class GetOvertimeLetterWithPaginationQueryHandler : IRequestHandler<GetOvertimeLettersWithPaginationQuery, PaginatedList<OvertimeLetterListItem>>
{
    private readonly IDataContext _context;

    public GetOvertimeLetterWithPaginationQueryHandler(IDataContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<OvertimeLetterListItem>> Handle(GetOvertimeLettersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var queries = from otl in _context.OvertimeLetters
                      join emp in _context.Employees on otl.EmployeeKey equals emp.Key
                      join com in _context.Companies on emp.CompanyKey equals com.Key
                      join apt in _context.ApprovalTransactions on otl.ApprovalTransactionKey equals apt.Key into approvalTransactionGroup
                      from approvalTransaction in approvalTransactionGroup.DefaultIfEmpty()
                      join at in _context.Attendances on emp.Key equals at.Key into attendanceGroup
                      from attendance in attendanceGroup.DefaultIfEmpty()
                      where otl.DeletedAt == null &&
                      //attendance.AttendanceDate == otl.DateSubmission &&
                      (request.CompanyKey.HasValue ? emp.CompanyKey == request.CompanyKey : true)
                      select new
                      {
                          OvertimeLetter = otl,
                          Employee = emp,
                          Company = com,
                          ApprovalTransaction = approvalTransaction,
                          Attendance = attendance
                      };

        string search = request.pagination.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.OvertimeLetter.Number, $"%{search}%") ||
                                         EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Employee.LastName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Employee.Code, $"%{search}%") ||
                                         EF.Functions.ILike(b.Company.Name, $"%{search}%") ||
                                         EF.Functions.ILike(b.OvertimeLetter.DateSubmission.Year.ToString(), $"%{search}%") ||
                                         EF.Functions.ILike(b.OvertimeLetter.DateSubmission.Month.ToString(), $"%{search}%") ||
                                         EF.Functions.ILike(b.OvertimeLetter.DateSubmission.Day.ToString(), $"%{search}%") ||
                                         EF.Functions.ILike(Enum.GetName(typeof(ApprovalStatus), b.OvertimeLetter.ApprovalStatus), $"%{search}%"));
        }

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x.OvertimeLetter));
        }

        var overtimeLetters = await queries.Skip((request.pagination.PageNumber - 1) * request.pagination.PageSize)
                                    .Take(request.pagination.PageSize)
                                    .Select(x => new
                                    {
                                        x.OvertimeLetter,
                                        x.Employee,
                                        x.Company,
                                        x.ApprovalTransaction,
                                        x.Attendance
                                    }).ToListAsync();

        var overtimeLetterList = overtimeLetters.Select(x => new OvertimeLetterListItem
        {
            Key = x.OvertimeLetter.Key,
            EmployeeKey = x.OvertimeLetter.EmployeeKey,
            DateSubmission = x.OvertimeLetter.DateSubmission,
            TimeIn = x.Attendance != null ? x.Attendance.In : TimeOnly.MinValue,
            TimeOut = x.Attendance != null ? x.Attendance.Out : TimeOnly.MinValue,
            OvertimeIn = x.OvertimeLetter.OvertimeIn,
            OvertimeOut = x.OvertimeLetter.OvertimeOut,
            Description = x.OvertimeLetter.Description,
            ApprovalStatus = x.OvertimeLetter.ApprovalStatus,
            StatusName = Enum.GetName(typeof(ApprovalStatus), x.OvertimeLetter.ApprovalStatus),
            Number = x.OvertimeLetter.Number,
            Employee = x.Employee,
            Company = x.Company
        }).ToList();

        return new PaginatedList<OvertimeLetterListItem>(overtimeLetterList, overtimeLetters.Count(), request.pagination.PageNumber, request.pagination.PageSize);
    }
}
#endregion
#endregion

#region "Get Overtime Letter By Id"
#region "Query"
public sealed record GetOvertimeLetterQuery(Guid Key) : IRequest<OvertimeLetterForm>;
#endregion
#region "Handler"
public sealed class GetOvertimeLetterQueryHandler(IDataContext _context,
                                                  IWebHostEnvironment _webHostEnvironment) : IRequestHandler<GetOvertimeLetterQuery, OvertimeLetterForm>
{
    public async Task<OvertimeLetterForm> Handle(GetOvertimeLetterQuery request, CancellationToken cancellationToken)
    {
        var overtimeLetter = await (from otl in _context.OvertimeLetters
                                    join emp in _context.Employees on otl.EmployeeKey equals emp.Key
                                    where otl.Key == request.Key
                                    select new OvertimeLetter
                                    {
                                        Key = otl.Key,
                                        EmployeeKey = otl.EmployeeKey,
                                        DateSubmission = otl.DateSubmission,
                                        OvertimeIn = otl.OvertimeIn,
                                        OvertimeOut = otl.OvertimeOut,
                                        Description = otl.Description,
                                        ApprovalStatus = otl.ApprovalStatus,
                                        Number = otl.Number,
                                        Documents = otl.Documents,
                                        Employee = emp
                                    }).FirstOrDefaultAsync();

        if (overtimeLetter == null)
            throw new Exception("Overtime Letter not found.");

        var approvalStamps = await (from apt in _context.ApprovalTransactions
                                    join ast in _context.ApprovalStamps on apt.Key equals ast.ApprovalTransactionKey
                                    join emp in _context.Employees on apt.EmployeeKey equals emp.Key
                                    join apr in _context.Approvers on ast.EmployeeKey equals apr.EmployeeKey into approverGroup
                                    from approver in approverGroup.DefaultIfEmpty()
                                    where apt.Key == overtimeLetter.ApprovalTransactionKey && apt.Category == ApprovalCategory.OvertimeLetter
                                    select new
                                    {
                                        ApprovalTransaction = apt,
                                        ApprovalStamps = ast,
                                        Approver = emp,
                                        Action = approver.Action,
                                        Level = approver.Level
                                    }).ToListAsync();

        var assets = new List<AssetForm>();
        if (overtimeLetter.Documents != null && overtimeLetter.Documents.Any())
        {
            var assetList = await _context.Assets
                                   .Where(x => overtimeLetter.Documents.Contains(x.Key))
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

        var viewModel = overtimeLetter.ConvertToViewModelOvertime();

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

#region "Save Overtime Letter"
#region "Command"
public sealed record SaveOvertimeLetterCommand(OvertimeLetterDto Form) : IRequest<Result>;
#endregion
#region "Handler"
public sealed class SaveOvertimeLetterCommandHandler(IDataContext _context,
                                                     IMediator _mediator,
                                                     IValidator<OvertimeLetterDto> _validator,
                                                     IApprovalTransactionRepository _repository) : IRequestHandler<SaveOvertimeLetterCommand, Result>
{
    public async Task<Result> Handle(SaveOvertimeLetterCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var employeeAttendance = await _context.EmployeesAttendances.FirstOrDefaultAsync(x => x.EmployeeKey == command.Form.EmployeeKey);
            if (employeeAttendance != null && employeeAttendance.OvertimeMode == OvertimeMode.No)
                return Result.Failure(new[] { "This employee is not allowed to apply for overtime." });

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

            var overtimeLetter = command.Form.ConvertToEntity();

            //Set documents
            overtimeLetter.Documents = documentKeys.ToArray();

            if (overtimeLetter.Key == Guid.Empty)
                overtimeLetter.Key = Guid.NewGuid();

            //Check if overtime letter is exists
            var existingOvertimeLetter = await _context.OvertimeLetters.FirstOrDefaultAsync(x => x.Key == overtimeLetter.Key && x.DeletedAt == null, cancellationToken);

            if (existingOvertimeLetter == null)
            {
                //Add new Overtime Letter
                _context.OvertimeLetters.Add(overtimeLetter);
            }
            else
            {
                //Update existing overtime letter
                overtimeLetter.CreatedAt = existingOvertimeLetter.CreatedAt;
                overtimeLetter.CreatedBy = existingOvertimeLetter.CreatedBy;
                _context.OvertimeLetters.Entry(existingOvertimeLetter).CurrentValues.SetValues(overtimeLetter);
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

#region "Delete Overtime Letter"
#region "Command"
public sealed record DeleteOvertimeLetterCommand(Guid Key) : IRequest<Result>;
#endregion
#region "Handler"
public sealed class DeleteOvertimeLetterCommandHandler(IDataContext _context,
                                                       IMediator _mediator) : IRequestHandler<DeleteOvertimeLetterCommand, Result>
{
    public async Task<Result> Handle(DeleteOvertimeLetterCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var overtimeLetter = await _context.OvertimeLetters.FirstOrDefaultAsync(x => x.Key == command.Key, cancellationToken);

            if (overtimeLetter == null)
                throw new Exception("Overtime Letter not found.");

            //Delete approval transaction if exists
            if (overtimeLetter.ApprovalTransactionKey != Guid.Empty)
            {
                var approvalResult = await _mediator.Send(new DeleteApprovalTransactionCommand(overtimeLetter.ApprovalTransactionKey), cancellationToken);
                if (!approvalResult.Succeeded)
                    throw new Exception(approvalResult.Errors.FirstOrDefault());
            }

            //Get document keys to delete
            var documentKeys = overtimeLetter.Documents;

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

            _context.OvertimeLetters.Remove(overtimeLetter);
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

#region "Get Overtime Letter Detail Report"
#region "Query"
public sealed record GetOvertimeLetterDetailReportQuery(OvertimeLetterDetailReportDto report, PaginationConfig paginationConfig) : IRequest<OvertimeLetterDetailReport>;
#endregion
#region "Handler"
public sealed class GetOvertimeLetterDetailReportQueryHandler(IAttendanceRepository _repository) : IRequestHandler<GetOvertimeLetterDetailReportQuery, OvertimeLetterDetailReport>
{
    public async Task<OvertimeLetterDetailReport> Handle(GetOvertimeLetterDetailReportQuery request, CancellationToken cancellationToken)
    {
        var overtimeLetterDetails = await _repository.GetOvertimeLetterDetailReport(request.report, request.paginationConfig);

        var viewModel = new OvertimeLetterDetailReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            OvertimeLetterDetailReports = overtimeLetterDetails.Items,
            PageNumber = overtimeLetterDetails.PageNumber,
            TotalCount = overtimeLetterDetails.TotalCount,
            TotalPages = overtimeLetterDetails.TotalPages
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Overtime Letter Detail Report"
#region "Query"
public sealed record GenerateOvertimeLetterDetailReportQuery(OvertimeLetterDetailReportDto report, PaginationConfig paginationConfig) : IRequest<byte[]>;
#endregion
#region "Handler"
public sealed class GenerateOvertimeLetterDetailReportQueryHandler(IAttendanceRepository _repository,
                                                                   IDocumentGenerator _documentGenerator) : IRequestHandler<GenerateOvertimeLetterDetailReportQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateOvertimeLetterDetailReportQuery request, CancellationToken cancellationToken)
    {
        var overtimeLetterDetails = await _repository.GetOvertimeLetterDetailReport(request.report, request.paginationConfig);

        var viewModel = new OvertimeLetterDetailReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            OvertimeLetterDetailReports = overtimeLetterDetails.Items,
            PageNumber = overtimeLetterDetails.PageNumber,
            TotalCount = overtimeLetterDetails.TotalCount,
            TotalPages = overtimeLetterDetails.TotalPages
        };

        return await _documentGenerator.GenerateOvertimeLetterDetailReportXlsx(viewModel);
    }
}
#endregion
#endregion
