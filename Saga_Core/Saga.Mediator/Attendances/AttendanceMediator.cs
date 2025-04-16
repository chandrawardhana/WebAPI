using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.Entities.Attendance;
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

namespace Saga.Mediator.Attendances.AttendanceMediator;

#region "Get List Attendance"
#region "Query"
public sealed record GetAttendancesQuery(Expression<Func<Attendance, bool>>[] wheres) : IRequest<IEnumerable<Attendance>>;
#endregion
#region "Handler"
public sealed class GetAttendancesQueryHandler(IDataContext _context) : IRequestHandler<GetAttendancesQuery, IEnumerable<Attendance>>
{
    public async Task<IEnumerable<Attendance>> Handle(GetAttendancesQuery request, CancellationToken cancellationToken)
    {
        var queries = from at in _context.Attendances
                      join emp in _context.Employees on at.EmployeeKey equals emp.Key
                      where at.DeletedAt == null
                      select new Attendance
                      {
                          EmployeeKey = at.EmployeeKey,
                          AttendanceDate = at.AttendanceDate,
                          In = at.In,
                          Out = at.Out,
                          ShiftName = at.ShiftName,
                          Status = at.Status,
                          Description = at.Description,
                          IsMobileApp = at.IsMobileApp,
                          IsFingerPrintMachine = at.IsFingerPrintMachine,
                          Latitude = at.Latitude,
                          Longitude = at.Longitude,
                          Employee = emp
                      };

        foreach (var filter in request.wheres)
        {
            queries = queries.Where(x => filter.Compile().Invoke(x));
        }

        var attendances = await queries.ToListAsync();

        return attendances;
    }
}
#endregion
#endregion

#region "Get List Attendance With Pagination"
#region "Query"
public sealed record GetAttendancesPaginationQuery(Expression<Func<Attendance, bool>>[] wheres, PaginationConfig pagination) : IRequest<PaginatedList<AttendanceListItem>>;
#endregion
#region "Handler"
public sealed class GetAttendancesPaginationQueryHandler(IDataContext _context) : IRequestHandler<GetAttendancesPaginationQuery, PaginatedList<AttendanceListItem>>
{
    public async Task<PaginatedList<AttendanceListItem>> Handle(GetAttendancesPaginationQuery request, CancellationToken cancellationToken)
    {
        var attendanceQuery = _context.Attendances.Where(x => x.DeletedAt == null);

        // Apply each filter to the employee query
        foreach (var filter in request.wheres)
        {
            attendanceQuery = attendanceQuery.Where(filter);
        }

        var queries = from at in attendanceQuery
                      join emp in _context.Employees on at.EmployeeKey equals emp.Key
                      join com in _context.Companies on emp.CompanyKey equals com.Key
                      join org in _context.Organizations on emp.OrganizationKey equals org.Key
                      join pos in _context.Positions on emp.PositionKey equals pos.Key
                      join ti in _context.Titles on emp.TitleKey equals ti.Key
                      //where at.DeletedAt == null
                      select new 
                      {
                          Attendance = at,
                          Employee = emp,
                          Company = com,
                          Organization = org,
                          Position = pos,
                          Title = ti
                      };

        string search = request.pagination.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Employee.Code, $"%{search}%") || 
                                         EF.Functions.ILike(b.Employee.FirstName, $"%{search}%") || 
                                         EF.Functions.ILike(b.Employee.LastName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Company.Name, $"%{search}%") ||
                                         EF.Functions.ILike(b.Organization.Name, $"%{search}%") ||
                                         EF.Functions.ILike(b.Position.Name, $"%{search}%") ||
                                         EF.Functions.ILike(b.Title.Name, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.ShiftName, $"%{search}%"));
        }

        //foreach (var filter in request.wheres)
        //{
        //    queries = queries.Where(x => filter.Compile().Invoke(x.Attendance));
        //}

        var attendances = await queries.Select(x => new AttendanceListItem
        {
            Key = x.Attendance.Key,
            EmployeeKey = x.Employee.Key,
            AttendanceDate = x.Attendance.AttendanceDate,
            In = x.Attendance.In,
            Out = x.Attendance.Out,
            ShiftName = x.Attendance.ShiftName,
            Status = x.Attendance.Status,
            Description = x.Attendance.Description,
            IsMobileApp = x.Attendance.IsMobileApp,
            IsFingerPrintMachine = x.Attendance.IsFingerPrintMachine,
            Latitude = x.Attendance.Latitude,
            Longitude = x.Attendance.Longitude,
            Employee = x.Employee,
            Company = x.Company,
            Organization = x.Organization,
            Position = x.Position,
            Title = x.Title
        }).PaginatedListAsync(request.pagination.PageNumber, request.pagination.PageSize);
        
        return await Task.FromResult(attendances);
    }
}
#endregion
#endregion

#region "Get Attendance Daily Report"
#region "Query"
public sealed record GetAttendanceDailyReportQuery(AttendanceDailyReportDto report, PaginationConfig pagination) : IRequest<AttendanceDailyReport>;
#endregion
#region "Handler"
public sealed class GetAttendanceDailyReportQueryHandler(IAttendanceRepository _repository) : IRequestHandler<GetAttendanceDailyReportQuery, AttendanceDailyReport>
{
    public async Task<AttendanceDailyReport> Handle(GetAttendanceDailyReportQuery request, CancellationToken cancellationToken)
    {
        var attendances = await _repository.GetAttendanceDailyReport(request.report, request.pagination);

        var viewModel = new AttendanceDailyReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            StartDate = request.report.StartDate,
            EndDate = request.report.EndDate,
            AttendancesData = attendances.Items,
            PageNumber = attendances.PageNumber,
            TotalCount = attendances.TotalCount,
            TotalPages = attendances.TotalPages
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Attendance Daily Report"
#region "Query"
public sealed record GenerateAttendanceDailyReportQuery(AttendanceDailyReportDto report, PaginationConfig pagination) : IRequest<byte[]>;
#endregion
#region "Handler"
public sealed class GenerateAttendanceDailyReportQueryHandler(IDocumentGenerator _documentGenerator,
                                                              IAttendanceRepository _repository) : IRequestHandler<GenerateAttendanceDailyReportQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateAttendanceDailyReportQuery request, CancellationToken cancellationToken)
    {
        var attendances = await _repository.GetAttendanceDailyReport(request.report, request.pagination);

        var viewModel = new AttendanceDailyReport
        {
            AttendancesData = attendances.Items
        };

        return await _documentGenerator.GenerateAttendanceDailyReportXlsx(viewModel);
    }
}
#endregion
#endregion

#region "Get Attendance Weekly Report"
#region "Query"
public sealed record GetAttendanceWeeklyReportQuery(AttendanceWeeklyReportDto report, PaginationConfig pagination) : IRequest<AttendanceWeeklyReport>;
#endregion
#region "Handler"
public sealed class GetAttendanceWeeklyReportQueryHandler(IAttendanceRepository _repository) : IRequestHandler<GetAttendanceWeeklyReportQuery, AttendanceWeeklyReport>
{
    public async Task<AttendanceWeeklyReport> Handle(GetAttendanceWeeklyReportQuery request, CancellationToken cancellationToken)
    {
        var attendances = await _repository.GetAttendanceWeeklyReport(request.report, request.pagination);

        var viewModel = new AttendanceWeeklyReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            StartDate = request.report.StartDate,
            AttendancesWeeklyData = attendances.Items,
            PageNumber = attendances.PageNumber,
            TotalCount = attendances.TotalCount,
            TotalPages = attendances.TotalPages
        };
        
        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Attendance Weekly Report"
#region "Query"
public sealed record GenerateAttendanceWeeklyReportQuery(AttendanceWeeklyReportDto report, PaginationConfig pagination) : IRequest<byte[]>;
#endregion
#region "Handler"
public sealed class GenerateAttendanceWeeklyReportQueryHandler(IDocumentGenerator _documentGenerator,
                                                               IAttendanceRepository _repository) : IRequestHandler<GenerateAttendanceWeeklyReportQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateAttendanceWeeklyReportQuery request, CancellationToken cancellationToken)
    {
        var attendances = await _repository.GetAttendanceWeeklyReport(request.report, request.pagination);

        var viewModel = new AttendanceWeeklyReport
        {
            StartDate = request.report.StartDate,
            AttendancesWeeklyData = attendances.Items
        };

        return await _documentGenerator.GenerateAttendanceWeeklyReportXlsx(viewModel);
    }
}
#endregion
#endregion

#region "Get Attendance Monthly Report"
#region "Query"
public sealed record GetAttendanceMonthlyReportQuery(AttendanceMonthlyReportDto report, PaginationConfig pagination) : IRequest<AttendanceMonthlyReport>;
#endregion
#region "Handler"
public sealed class GetAttendanceMonthlyReportQueryHandler(IAttendanceRepository _repository) : IRequestHandler<GetAttendanceMonthlyReportQuery, AttendanceMonthlyReport>
{
    public async Task<AttendanceMonthlyReport> Handle(GetAttendanceMonthlyReportQuery request, CancellationToken cancellationToken)
    {
        var attendances = await _repository.GetAttendanceMonthlyReport(request.report, request.pagination);

        var viewModel = new AttendanceMonthlyReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            AttendancesMonthlyData = attendances.Items,
            PageNumber = attendances.PageNumber,
            TotalCount = attendances.TotalCount,
            TotalPages = attendances.TotalPages
        };
        
        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Attendance Monthly Report"
#region "Query"
public sealed record GenerateAttendanceMonthlyReportQuery(AttendanceMonthlyReportDto report, PaginationConfig pagination): IRequest<byte[]>;
#endregion
#region "Handler"
public sealed class GenerateAttendanceMonthlyReportQueryHandler(IDocumentGenerator _documentGenerator,
                                                                IAttendanceRepository _repository) : IRequestHandler<GenerateAttendanceMonthlyReportQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateAttendanceMonthlyReportQuery request, CancellationToken cancellationToken)
    {
        var attendances = await _repository.GetAttendanceMonthlyReport(request.report, request.pagination);

        var viewModel = new AttendanceMonthlyReport
        {
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            AttendancesMonthlyData = attendances.Items
        };
        
        return await _documentGenerator.GenerateAttendanceMonthlyReportXlsx(viewModel);
    }
}
#endregion
#endregion

#region "Get Attendance Recapitulation Report"
#region "Query"
public sealed record GetAttendanceRecapitulationReportQuery(AttendanceRecapitulationReportDto report, PaginationConfig pagination): IRequest<AttendanceRecapitulationReport>;
#endregion
#region "Handler"
public sealed class GetAttendanceRecapitulationReportQueryHandler(IAttendanceRepository _repository) : IRequestHandler<GetAttendanceRecapitulationReportQuery, AttendanceRecapitulationReport>
{
    public async Task<AttendanceRecapitulationReport> Handle(GetAttendanceRecapitulationReportQuery request, CancellationToken cancellationToken)
    {
        var attendances = await _repository.GetAttendanceRecapitulationReport(request.report, request.pagination);

        var viewModel = new AttendanceRecapitulationReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            AttendancesRecapitulationData = attendances.Items,
            PageNumber = attendances.PageNumber,
            TotalCount = attendances.TotalCount,
            TotalPages = attendances.TotalPages
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Attendance Recapitulation Report"
#region "Query"
public sealed record GenerateAttendanceRecapitulationReportQuery(AttendanceRecapitulationReportDto report, PaginationConfig pagination): IRequest<byte[]>;
#endregion
#region "Handler"
public sealed class GenerateAttendanceRecapitulationReportQueryHandler(IDocumentGenerator _documentGenerator,
                                                                       IAttendanceRepository _repository) : IRequestHandler<GenerateAttendanceRecapitulationReportQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateAttendanceRecapitulationReportQuery request, CancellationToken cancellationToken)
    {
        var attendances = await _repository.GetAttendanceRecapitulationReport(request.report, request.pagination);

        var viewModel = new AttendanceRecapitulationReport
        {
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            AttendancesRecapitulationData = attendances.Items
        };

        return await _documentGenerator.GenerateAttendanceRecapitulationReportXlsx(viewModel);
    }
}
#endregion
#endregion

#region "Get Attendance Permit Detail Report"
#region "Query"
public sealed record GetPermitDetailReportQuery(PermitDetailReportDto report, PaginationConfig paginationConfig) : IRequest<PermitDetailReport>;
#endregion
#region "Handler"
public sealed class GetPermitDetailReportQueryHandler(IAttendanceRepository _repository) : IRequestHandler<GetPermitDetailReportQuery, PermitDetailReport>
{
    public async Task<PermitDetailReport> Handle(GetPermitDetailReportQuery request, CancellationToken cancellationToken)
    {
        var earlyOutDetails = Enumerable.Empty<EarlyOutDetailReportData>();
        var outPermitDetails = Enumerable.Empty<OutPermitDetailReportData>();
        int pageNumber = 0;
        int totalPages = 0;
        int totalCount = 0;

        if (request.report.Category == PermitReportCategory.EarlyOutPermit)
        {
            var earlyOutDetailsData = await _repository.GetEarlyOutDetailReport(request.report, request.paginationConfig);

            earlyOutDetails = earlyOutDetailsData.Items;
            pageNumber = earlyOutDetailsData.PageNumber;
            totalPages = earlyOutDetailsData.TotalPages;
            totalCount = earlyOutDetailsData.TotalCount;
        }
        else
        {
            var outPermitDetailsData = await _repository.GetOutPermitDetailReport(request.report, request.paginationConfig);

            outPermitDetails = outPermitDetailsData.Items;
            pageNumber = outPermitDetailsData.PageNumber;
            totalPages = outPermitDetailsData.TotalPages;
            totalCount = outPermitDetailsData.TotalCount;
        }

        var viewModel = new PermitDetailReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            Category = request.report.Category,
            EarlyOutDetailReports = earlyOutDetails,
            OutPermitDetailReports = outPermitDetails,
            PageNumber = pageNumber,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Attendance Permit Detail Report"
#region "Query"
public sealed record GeneratePermitDetailReportQuery(PermitDetailReportDto report, PaginationConfig paginationConfig) : IRequest<byte[]>;
public sealed class GeneratePermitDetailReportQueryHandler(IAttendanceRepository _repository,
                                                           IDocumentGenerator _documentGenerator) : IRequestHandler<GeneratePermitDetailReportQuery, byte[]>
{
    public async Task<byte[]> Handle(GeneratePermitDetailReportQuery request, CancellationToken cancellationToken)
    {
        var earlyOutDetails = Enumerable.Empty<EarlyOutDetailReportData>();
        var outPermitDetails = Enumerable.Empty<OutPermitDetailReportData>();
        int pageNumber = 0;
        int totalPages = 0;
        int totalCount = 0;
        byte[] result = Array.Empty<byte>();

        if (request.report.Category == PermitReportCategory.EarlyOutPermit)
        {
            var earlyOutDetailsData = await _repository.GetEarlyOutDetailReport(request.report, request.paginationConfig);

            earlyOutDetails = earlyOutDetailsData.Items;
            pageNumber = earlyOutDetailsData.PageNumber;
            totalPages = earlyOutDetailsData.TotalPages;
            totalCount = earlyOutDetailsData.TotalCount;
        }
        else
        {
            var outPermitDetailsData = await _repository.GetOutPermitDetailReport(request.report, request.paginationConfig);

            outPermitDetails = outPermitDetailsData.Items;
            pageNumber = outPermitDetailsData.PageNumber;
            totalPages = outPermitDetailsData.TotalPages;
            totalCount = outPermitDetailsData.TotalCount;
        }

        var viewModel = new PermitDetailReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            Category = request.report.Category,
            EarlyOutDetailReports = earlyOutDetails,
            OutPermitDetailReports = outPermitDetails,
            PageNumber = pageNumber,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        if (request.report.Category == PermitReportCategory.EarlyOutPermit && earlyOutDetails.Any())
            result = await _documentGenerator.GenerateAttendanceEarlyOutDetailReportXlsx(viewModel);
        else if (request.report.Category == PermitReportCategory.OutOffice && outPermitDetails.Any())
            result = await _documentGenerator.GenerateAttendanceOutPermitDetailReportXlsx(viewModel);

        return result;
    }
}
#endregion
#endregion

#region "Get Attendance Shift Schedule Detail Report"
#region "Query"
public sealed record GetShiftScheduleReportQuery(ShiftScheduleDetailReportDto report, PaginationConfig paginationConfig) : IRequest<ShiftScheduleDetailReport>;
#endregion
#region "Handler"
public sealed class GetShiftScheduleReportQueryHandler(IAttendanceRepository _repository) : IRequestHandler<GetShiftScheduleReportQuery, ShiftScheduleDetailReport>
{
    public async Task<ShiftScheduleDetailReport> Handle(GetShiftScheduleReportQuery request, CancellationToken cancellationToken)
    {
        var shiftScheduleDetails = await _repository.GetShiftScheduleDetailReport(request.report, request.paginationConfig);

        var viewModel = new ShiftScheduleDetailReport
        {
            EmployeeKey = request.report.EmployeeKey ?? Guid.Empty,
            CompanyKey = request.report.CompanyKey ?? Guid.Empty,
            OrganizationKey = request.report.OrganizationKey ?? Guid.Empty,
            PositionKey = request.report.PositionKey ?? Guid.Empty,
            TitleKey = request.report.TitleKey ?? Guid.Empty,
            DocumentGeneratorFormat = request.report.DocumentGeneratorFormat ?? DocumentGeneratorFormat.Xlsx,
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            ShiftScheduleDetailReports = shiftScheduleDetails.Items,
            PageNumber = shiftScheduleDetails.PageNumber,
            TotalCount = shiftScheduleDetails.TotalCount,
            TotalPages = shiftScheduleDetails.TotalPages
        };

        return viewModel;
    }
}
#endregion
#endregion

#region "Generate Attendance Shift Schedule Detail Report"
#region "Query"
public sealed record GenerateShiftScheduleReportQuery(ShiftScheduleDetailReportDto report, PaginationConfig paginationConfig) : IRequest<byte[]>;
#endregion
#region "Handler"
public sealed class GenerateShiftScheduleReportQueryHandler(IDocumentGenerator _documentGenerator,
                                                            IAttendanceRepository _repository) : IRequestHandler<GenerateShiftScheduleReportQuery, byte[]>
{
    public async Task<byte[]> Handle(GenerateShiftScheduleReportQuery request, CancellationToken cancellationToken)
    {
        var shiftScheduleDetails = await _repository.GetShiftScheduleDetailReport(request.report, request.paginationConfig);

        var viewModel = new ShiftScheduleDetailReport
        {
            SelectedMonth = request.report.SelectedMonth,
            SelectedYear = request.report.SelectedYear,
            ShiftScheduleDetailReports = shiftScheduleDetails.Items,
        };

        return await _documentGenerator.GenerateShiftScheduleDetailReportXlsx(viewModel);
    }
}
#endregion
#endregion


#region Attendacne Calculation
public sealed record AttendanceCalculationCommand(AttendanceMonthlyReportDto filter)
    : IRequest<IEnumerable<Result>>;

public sealed class AttendanceCalculationCommandHandler(
    IDataContext _context,
    IAttendanceService _attService) : IRequestHandler<AttendanceCalculationCommand, IEnumerable<Result>>
{
    public async Task<IEnumerable<Result>> Handle(AttendanceCalculationCommand request, CancellationToken cancellationToken)
    {
        var filter = request.filter;

        List<Result> results = [];

        // find employee by empKey / companyKey / orgKey / PostionKey / titleKey

        // var companyKeys = employee.select( x => x.companykey).toarray().distinct();

        // find master cutoffs by companyKeys => distinct
        // loop cut
        // cari tanggal awal & tanggal akhir dr cutoff berdasarkan bulan

        // 16 - 15
        // 21 - 20

        // 16 - 20 => 16 Feb - 20 Mar


        // Bulan April => 21 Mar - 20 Apr

        // end loop cutoff

        //var resultCalculate = await _attService.CalculationAttendanceAsync(employee, (dateStart, dateEnd) );

        return await Task.FromResult(results);
    }
}
#endregion