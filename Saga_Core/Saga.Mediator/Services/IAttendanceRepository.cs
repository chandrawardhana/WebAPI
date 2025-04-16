using Microsoft.EntityFrameworkCore;
using Saga.Domain.Dtos.Attendances;
using Saga.Domain.ViewModels.Attendances;
using Saga.DomainShared.Models;
using Saga.Persistence.Context;
using Saga.Persistence.Extensions;
using Saga.Persistence.Models;
using MediatR;
using Saga.Mediator.Attendances.CutOffMediator;
using Saga.DomainShared.Helpers;

namespace Saga.Mediator.Services;

public interface IAttendanceRepository
{
    Task<PaginatedList<AttendanceReportData>> GetAttendanceDailyReport(AttendanceDailyReportDto report, PaginationConfig paginationConfig);
    Task<PaginatedList<WeeklyAttendanceReportData>> GetAttendanceWeeklyReport(AttendanceWeeklyReportDto report, PaginationConfig paginationConfig);
    Task<PaginatedList<WeeklyAttendanceReportData>> GetAttendanceMonthlyReport(AttendanceMonthlyReportDto report, PaginationConfig paginationConfig);
    Task<PaginatedList<RecapitulationAttendanceReportData>> GetAttendanceRecapitulationReport(AttendanceRecapitulationReportDto report, PaginationConfig paginationConfig);
    Task<PaginatedList<LateDetailReportData>> GetLateDetailReport(LateDetailReportDto report, PaginationConfig paginationConfig);
    Task<PaginatedList<LeaveDetailReportData>> GetLeaveDetailReport(LeaveDetailReportDto report, PaginationConfig paginationConfig);
    Task<PaginatedList<EarlyOutDetailReportData>> GetEarlyOutDetailReport(PermitDetailReportDto report, PaginationConfig paginationConfig);
    Task<PaginatedList<OutPermitDetailReportData>> GetOutPermitDetailReport(PermitDetailReportDto report, PaginationConfig paginationConfig);
    Task<PaginatedList<ShiftScheduleDetailReportData>> GetShiftScheduleDetailReport(ShiftScheduleDetailReportDto report, PaginationConfig paginationConfig);
    Task<PaginatedList<OvertimeLetterDetailReportData>> GetOvertimeLetterDetailReport(OvertimeLetterDetailReportDto report, PaginationConfig paginationConfig);
}

public class AttendanceRepository(IDataContext _context,
                                  IMediator _mediator) : IAttendanceRepository
{
    public async Task<PaginatedList<AttendanceReportData>> GetAttendanceDailyReport(AttendanceDailyReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.StartDate.HasValue && !report.EndDate.HasValue)
            throw new ArgumentException("Start date and End Date is required for daily report.");

        var startDateStr = report.StartDate.Value.ToInvariantString();
        var endDateStr = report.EndDate.Value.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                                    SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                                    WHERE ""DeletedAt"" IS NULL 
                                                                    AND ""AttendanceDate"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                                    AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                                                ")
                      where (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true) &&
                            (report.StartDate.HasValue ? att.AttendanceDate >= report.StartDate : true) &&
                            (report.EndDate.HasValue ? att.AttendanceDate <= report.EndDate : true)
                      select new
                      {
                          Attendance = att,
                      };

        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.ShiftName, $"%{search}%"));
        }

        var attendances = await queries.ToListAsync();

        var attendanceList = attendances.Select(x => new AttendanceReportData
        {
            EmployeeID = x.Attendance.EmployeeCode,
            EmployeeName = x.Attendance.EmployeeName,
            CompanyName = x.Attendance.CompanyName,
            OrganizationName = x.Attendance.OrganizationName,
            PositionName = x.Attendance.PositionName,
            TitleName = x.Attendance.TitleName,
            AttendanceDate = x.Attendance.AttendanceDate,
            AttendanceDay = x.Attendance.AttendanceDay,
            In = x.Attendance.In,
            Out = x.Attendance.Out,
            ShiftName = x.Attendance.ShiftName,
            WorkingHour = x.Attendance.WorkingHour,
            Description = x.Attendance.Description ?? String.Empty,
            AttendanceStatus = x.Attendance.Status,
            IsLateDocument = x.Attendance.IsLateDocument
        }).ToList();

        return new PaginatedList<AttendanceReportData>(
           attendanceList,
           attendanceList.Count,
           paginationConfig.PageNumber,
           paginationConfig.PageSize);
    }


    public async Task<PaginatedList<WeeklyAttendanceReportData>> GetAttendanceWeeklyReport(AttendanceWeeklyReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.StartDate.HasValue)
            throw new ArgumentException("Start date is required for weekly report.");

        var endDate = report.StartDate.Value.AddDays(6);
        var startDateStr = report.StartDate.Value.ToInvariantString();
        var endDateStr = endDate.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                                    SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                                    WHERE ""DeletedAt"" IS NULL 
                                                                    AND ""AttendanceDate"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                                    AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                                                ")
                      where (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true)
                      select new
                      {
                          Attendance = att
                      };

        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%"));
        }

        var weeklyData = await queries.ToListAsync();

        var weeklyDataList = weeklyData.Select(x => new WeeklyAttendanceReportData
        {
            NIK = x.Attendance.EmployeeCode,
            EmployeeName = x.Attendance.EmployeeName,
            CompanyName = x.Attendance.CompanyName,
            OrganizationName = x.Attendance.OrganizationName,
            PositionName = x.Attendance.PositionName,
            TitleName = x.Attendance.TitleName,
            DailyAttendances = new List<DailyAttendance>
            {
                new DailyAttendance
                {
                    AttendanceDate = x.Attendance.AttendanceDate,
                    In = x.Attendance.In,
                    Out = x.Attendance.Out,
                    Status = x.Attendance.Status,
                    IsLateDocument = x.Attendance.IsLateDocument
                }
            }
        }).ToList();

        return new PaginatedList<WeeklyAttendanceReportData>(
            weeklyDataList,
            weeklyDataList.Count,
            paginationConfig.PageNumber,
            paginationConfig.PageSize);
    }

    public async Task<PaginatedList<WeeklyAttendanceReportData>> GetAttendanceMonthlyReport(AttendanceMonthlyReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || !report.CompanyKey.HasValue)
            throw new ArgumentException("Month, year and company are required for monthly report.");

        //Calculate date range for the monthly report
        var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
        var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
        var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

        var startDateStr = startDate.ToInvariantString();
        var endDateStr = endDate.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                                    SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                                    WHERE ""DeletedAt"" IS NULL 
                                                                    AND ""AttendanceDate"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                                    AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                                                ")
                      where (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true)
                      select new
                      {
                          Attendance = att
                      };

        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%"));
        }


        var monthlyData = await queries.ToListAsync();

        var monthlyDataList = monthlyData.Select(x => new WeeklyAttendanceReportData
        {
            NIK = x.Attendance.EmployeeCode,
            EmployeeName = x.Attendance.EmployeeName,
            CompanyName = x.Attendance.CompanyName,
            OrganizationName = x.Attendance.OrganizationName,
            PositionName = x.Attendance.PositionName,
            TitleName = x.Attendance.TitleName,
            DailyAttendances = new List<DailyAttendance>
            {
                new DailyAttendance
                {
                    AttendanceDate = x.Attendance.AttendanceDate,
                    In = x.Attendance.In,
                    Out = x.Attendance.Out,
                    Status = x.Attendance.Status,
                    IsLateDocument = x.Attendance.IsLateDocument
                }
            }
        }).ToList();

        return new PaginatedList<WeeklyAttendanceReportData>(
            monthlyDataList,
            monthlyDataList.Count,
            paginationConfig.PageNumber,
            paginationConfig.PageSize);
    }

    public async Task<PaginatedList<RecapitulationAttendanceReportData>> GetAttendanceRecapitulationReport(AttendanceRecapitulationReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || !report.CompanyKey.HasValue)
            throw new ArgumentException("Month, year, and company are required for recapitulation report.");

        //Calculate date range for the recapitulation report
        var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
        var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
        var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

        var startDateStr = startDate.ToInvariantString();
        var endDateStr = endDate.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                                    SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                                    WHERE ""DeletedAt"" IS NULL 
                                                                    AND ""AttendanceDate"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                                    AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                                                ")
                      where (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true)
                      select new
                      {
                          Attendance = att
                      };

        // Apply search filter
        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%"));
        }

        var attendances = await queries.ToListAsync();

        // Group by employee and calculate totals
        var groupedEmployees = attendances
            .GroupBy(a => new
            {
                a.Attendance.EmployeeKey,
                a.Attendance.EmployeeCode,
                a.Attendance.EmployeeName,
                a.Attendance.CompanyName,
                a.Attendance.OrganizationName,
                a.Attendance.PositionName,
                a.Attendance.TitleName
            });

        var recapDataList = new List<RecapitulationAttendanceReportData>();

        foreach (var group in groupedEmployees)
        {
            var attendancesInGroup = group.ToList();

            var dailyRecap = attendancesInGroup.Select(a => new DailyRecap
            {
                Date = a.Attendance.AttendanceDate,
                Code = a.Attendance.AttendanceCode
            }).ToList();

            double totalWorkingHours = attendancesInGroup.Sum(a => ParseWorkingHour(a.Attendance.WorkingHour));

            int workEntries = attendancesInGroup.Count(a => !a.Attendance.IsAlpha.GetValueOrDefault());
            int alphas = attendancesInGroup.Sum(a => a.Attendance.CountAlpha.GetValueOrDefault());

            var leaveTotals = attendancesInGroup.Where(a => !string.IsNullOrEmpty(a.Attendance.LeaveCode))
                                                .GroupBy(a => a.Attendance.LeaveCode)
                                                .ToDictionary(g => g.Key, g => g.Count());

            recapDataList.Add(new RecapitulationAttendanceReportData
            {
                NIK = group.Key.EmployeeCode,
                EmployeeName = group.Key.EmployeeName,
                CompanyName = group.Key.CompanyName,
                OrganizationName = group.Key.OrganizationName,
                PositionName = group.Key.PositionName,
                TitleName = group.Key.TitleName,
                DailyRecaps = dailyRecap,
                Totals = new AttendanceTotals
                {
                    WorkingHours = totalWorkingHours,
                    WorkEntries = workEntries,
                    Alphas = alphas,
                    LeaveTotals = leaveTotals
                }
            });
        }

        return new PaginatedList<RecapitulationAttendanceReportData>(
            recapDataList,
            recapDataList.Count,
            paginationConfig.PageNumber,
            paginationConfig.PageSize);
    }

    public async Task<PaginatedList<LateDetailReportData>> GetLateDetailReport(LateDetailReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || !report.CompanyKey.HasValue)
            throw new ArgumentException("Month, year and company are required for late detail report.");

        //Calculate date range for the late detail report
        var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
        var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
        var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

        var startDateStr = startDate.ToInvariantString();
        var endDateStr = endDate.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                                    SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                                    WHERE ""DeletedAt"" IS NULL 
                                                                    AND ""AttendanceDate"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                                    AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                                                ")
                      where (att.IsLatePermit == true || (att.In > att.ShiftInTime)) &&
                            (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true)
                      select new
                      {
                          Attendance = att
                      };

        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                         EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%"));
        }

        var latePermitDetails = await queries.ToListAsync();

        var latePermitDetailList = latePermitDetails.Select(x => new LateDetailReportData
        {
            NIK = x.Attendance.EmployeeCode,
            EmployeeName = x.Attendance.EmployeeName,
            CompanyName = x.Attendance.CompanyName,
            OrganizationName = x.Attendance.OrganizationName,
            PositionName = x.Attendance.PositionName,
            TitleName = x.Attendance.TitleName,
            LatePermitDate = x.Attendance.AttendanceDate,
            TimeIn = x.Attendance.IsLatePermit == true ? x.Attendance.TimeIn : x.Attendance.In,
            IsLateDocument = x.Attendance.IsLateDocument,
            Date = x.Attendance.AttendanceDate,
            TimeOut = x.Attendance.Out,
            Late = x.Attendance.TotalLate ?? TimeOnly.MinValue,
            IsPermited = x.Attendance.IsLatePermit ?? false,
            WorkingHour = x.Attendance.WorkingHour,
            Description = x.Attendance.IsLatePermit == true ? x.Attendance.LatePermitReason : x.Attendance.Description
        }).ToList();

        return new PaginatedList<LateDetailReportData>(
            latePermitDetailList,
            latePermitDetailList.Count,
            paginationConfig.PageNumber,
            paginationConfig.PageSize);
    }

    public async Task<PaginatedList<LeaveDetailReportData>> GetLeaveDetailReport(LeaveDetailReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || !report.CompanyKey.HasValue)
            throw new ArgumentException("Month, year and company are required for recapitulation report.");

        //Calculate date range for the leave detail report
        var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
        var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
        var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

        var startDateStr = startDate.ToInvariantString();
        var endDateStr = endDate.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                WHERE ""DeletedAt"" IS NULL 
                                                AND ""LeaveDateStart"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                          ")
                      where att.IsLeaveSubmission == true &&
                            (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true)
                      select new
                      {
                          Attendance = att
                      };

        // Apply search filter
        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%"));
        }

        var attendances = await queries.OrderBy(x => x.Attendance.EmployeeCode).ThenBy(x => x.Attendance.AttendanceDate).ToListAsync();

        // Group by employee and calculate totals
        //var groupedEmployees = attendances
        //    .GroupBy(a => new
        //    {
        //        a.Attendance.EmployeeKey,
        //        a.Attendance.EmployeeCode,
        //        a.Attendance.EmployeeName,
        //        a.Attendance.CompanyName,
        //        a.Attendance.OrganizationName,
        //        a.Attendance.PositionName,
        //        a.Attendance.TitleName
        //    });

        //var leaveDetailList = new List<LeaveDetailReportData>();

        //foreach (var group in groupedEmployees)
        //{
        //    var attendancesInGroup = group.ToList();

        //    var dailyRecap = attendancesInGroup.Select(a => new DailyRecap
        //    {
        //        Date = a.Attendance.AttendanceDate,
        //        Code = a.Attendance.AttendanceCode
        //    }).ToList();

        //    var leaveTotals = attendancesInGroup.Where(a => !string.IsNullOrEmpty(a.Attendance.LeaveCode))
        //                                        .GroupBy(a => a.Attendance.LeaveCode)
        //                                        .ToDictionary(g => g.Key, g => g.Count());

        //    leaveDetailList.Add(new LeaveDetailReportData
        //    {
        //        NIK = group.Key.EmployeeCode,
        //        EmployeeName = group.Key.EmployeeName,
        //        CompanyName = group.Key.CompanyName,
        //        OrganizationName = group.Key.OrganizationName,
        //        PositionName = group.Key.PositionName,
        //        TitleName = group.Key.TitleName,
        //        DailyRecaps = dailyRecap,
        //        LeaveTotals = leaveTotals
        //    });
        //}

        var leaveDetailList = attendances.Select(x => new LeaveDetailReportData
                                          {
                                              NIK = x.Attendance.EmployeeCode,
                                              EmployeeName = x.Attendance.EmployeeName,
                                              CompanyName = x.Attendance.CompanyName,
                                              OrganizationName = x.Attendance.OrganizationName,
                                              PositionName = x.Attendance.PositionName,
                                              TitleName = x.Attendance.TitleName,
                                              Date = x.Attendance.AttendanceDate,
                                              LeaveName = x.Attendance.LeaveName,
                                              Description = x.Attendance.LeaveDescription
                                          }).ToList();

        return new PaginatedList<LeaveDetailReportData>(
            leaveDetailList,
            leaveDetailList.Count,
            paginationConfig.PageNumber,
            paginationConfig.PageSize);
    }

    public async Task<PaginatedList<EarlyOutDetailReportData>> GetEarlyOutDetailReport(PermitDetailReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || !report.CompanyKey.HasValue)
            throw new ArgumentException("Month, year and company are required for early out detail report.");

        //Calculate date range for the early out detail report
        var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
        var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
        var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

        var startDateStr = startDate.ToInvariantString();
        var endDateStr = endDate.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                                    SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                                    WHERE ""DeletedAt"" IS NULL 
                                                                    AND ""AttendanceDate"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                                    AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                                               ")
                      where att.IsEarlyOutPermit == true &&
                            (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true)
                      select new
                      {
                          Attendance = att
                      };

        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.EarlyOutReason, $"%{search}%"));
        }

        var earlyOutDetails = await queries.ToListAsync();

        var earlyOutDetailList = earlyOutDetails.Select(x => new EarlyOutDetailReportData
        {
            NIK = x.Attendance.EmployeeCode,
            EmployeeName = x.Attendance.EmployeeName,
            CompanyName = x.Attendance.CompanyName,
            OrganizationName = x.Attendance.OrganizationName,
            PositionName = x.Attendance.PositionName,
            TitleName = x.Attendance.TitleName,
            DateSubmission = x.Attendance.AttendanceDate,
            TimeOut = x.Attendance.EarlyOutPermitTimeOut ?? TimeOnly.MinValue,
            Reason = x.Attendance.EarlyOutReason
        }).ToList();

        return new PaginatedList<EarlyOutDetailReportData>(
            earlyOutDetailList,
            earlyOutDetailList.Count,
            paginationConfig.PageNumber,
            paginationConfig.PageSize);
    }

    public async Task<PaginatedList<OutPermitDetailReportData>> GetOutPermitDetailReport(PermitDetailReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || !report.CompanyKey.HasValue)
            throw new ArgumentException("Month, year and company are required for out permit detail report.");

        //Calculate date range for the out permit detail report
        var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
        var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
        var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

        var startDateStr = startDate.ToInvariantString();
        var endDateStr = endDate.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                WHERE ""DeletedAt"" IS NULL 
                                                AND ""AttendanceDate"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                          ")
                      where att.IsOutPermit == true &&
                            (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true)
                      select new
                      {
                          Attendance = att
                      };

        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.OutPermitReason, $"%{search}%"));
        }

        var outPermitDetails = await queries.ToListAsync();

        var outPermitDetailList = outPermitDetails.Select(x => new OutPermitDetailReportData
        {
            NIK = x.Attendance.EmployeeCode,
            EmployeeName = x.Attendance.EmployeeName,
            CompanyName = x.Attendance.CompanyName,
            OrganizationName = x.Attendance.OrganizationName,
            PositionName = x.Attendance.PositionName,
            TitleName = x.Attendance.TitleName,
            DateSubmission = x.Attendance.AttendanceDate,
            TimeOut = x.Attendance.OutPermitTimeOut ?? TimeOnly.MinValue,
            BackToOffice = x.Attendance.OutPermitBackToOffice ?? TimeOnly.MinValue,
            Reason = x.Attendance.OutPermitReason
        }).ToList();

        return new PaginatedList<OutPermitDetailReportData>(
            outPermitDetailList,
            outPermitDetailList.Count,
            paginationConfig.PageNumber,
            paginationConfig.PageSize);
    }

    public async Task<PaginatedList<ShiftScheduleDetailReportData>> GetShiftScheduleDetailReport(ShiftScheduleDetailReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || !report.CompanyKey.HasValue)
            throw new ArgumentException("Month, year and company are required for shift schedule detail report.");

        //Calculate date range for the shift schedule detail report
        var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
        var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
        var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

        var startDateStr = startDate.ToInvariantString();
        var endDateStr = endDate.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                                    SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                                    WHERE ""DeletedAt"" IS NULL 
                                                                    AND ""AttendanceDate"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                                    AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                                               ")
                      where (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true)
                      select new
                      {
                          Attendance = att
                      };

        // Apply search filter
        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%"));
        }

        var attendances = await queries.ToListAsync();

        // Group by employee and calculate totals
        var groupedEmployees = attendances
            .GroupBy(a => new
            {
                a.Attendance.EmployeeKey,
                a.Attendance.EmployeeCode,
                a.Attendance.EmployeeName,
                a.Attendance.CompanyName,
                a.Attendance.OrganizationName,
                a.Attendance.PositionName,
                a.Attendance.TitleName
            });

        var shiftScheduleDetailList = new List<ShiftScheduleDetailReportData>();

        foreach (var group in groupedEmployees)
        {
            var attendancesInGroup = group.ToList();

            var dailyShiftSchedule = attendancesInGroup.Select(a => new DailyShiftSchedule
            {
                Date = a.Attendance.AttendanceDate,
                ShiftName = a.Attendance.ShiftName
            }).ToList();

            shiftScheduleDetailList.Add(new ShiftScheduleDetailReportData
            {
                NIK = group.Key.EmployeeCode,
                EmployeeName = group.Key.EmployeeName,
                CompanyName = group.Key.CompanyName,
                OrganizationName = group.Key.OrganizationName,
                PositionName = group.Key.PositionName,
                TitleName = group.Key.TitleName,
                DailyShiftSchedules = dailyShiftSchedule
            });
        }

        return new PaginatedList<ShiftScheduleDetailReportData>(
            shiftScheduleDetailList,
            shiftScheduleDetailList.Count,
            paginationConfig.PageNumber,
            paginationConfig.PageSize);
    }

    public async Task<PaginatedList<OvertimeLetterDetailReportData>> GetOvertimeLetterDetailReport(OvertimeLetterDetailReportDto report, PaginationConfig paginationConfig)
    {
        if (!report.SelectedMonth.HasValue || !report.SelectedYear.HasValue || !report.CompanyKey.HasValue)
            throw new ArgumentException("Month, year and company are required for overtime letter detail report.");

        //Calculate date range for the overtime letter detail report
        var cutoff = await GetCutOffConfiguration(report.CompanyKey.Value, report.SelectedYear.Value);
        var (startDay, endDay) = CutOffDateRange.GetCutOffDays(cutoff, report.SelectedMonth.Value);
        var (startDate, endDate) = CutOffDateRange.CalculateCutOffDateRange(report.SelectedMonth.Value, report.SelectedYear.Value, startDay, endDay);

        var startDateStr = startDate.ToInvariantString();
        var endDateStr = endDate.ToInvariantString();

        var queries = from att in _context.Attendances
                                          .FromSqlInterpolated($@"
                                                                    SELECT * FROM ""Attendance"".""tbtattendance"" 
                                                                    WHERE ""DeletedAt"" IS NULL 
                                                                    AND ""AttendanceDate"" BETWEEN TO_DATE({startDateStr}, 'YYYY-MM-DD') 
                                                                    AND TO_DATE({endDateStr}, 'YYYY-MM-DD')
                                                               ")
                      where att.IsOvertimeLetter == true &&
                            (report.EmployeeKey.HasValue && report.EmployeeKey != Guid.Empty ? att.EmployeeKey == report.EmployeeKey : true) &&
                            (report.CompanyKey.HasValue && report.CompanyKey != Guid.Empty ? att.CompanyKey == report.CompanyKey : true) &&
                            (report.OrganizationKey.HasValue && report.OrganizationKey != Guid.Empty ? att.OrganizationKey == report.OrganizationKey : true) &&
                            (report.PositionKey.HasValue && report.PositionKey != Guid.Empty ? att.PositionKey == report.PositionKey : true) &&
                            (report.TitleKey.HasValue && report.TitleKey != Guid.Empty ? att.TitleKey == report.TitleKey : true)
                      select new
                      {
                          Attendance = att
                      };

        string search = paginationConfig.Find;
        if (!string.IsNullOrEmpty(search))
        {
            queries = queries.Where(b => EF.Functions.ILike(b.Attendance.EmployeeCode, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.EmployeeName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.CompanyName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.OrganizationName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.PositionName, $"%{search}%") ||
                                       EF.Functions.ILike(b.Attendance.TitleName, $"%{search}%") ||
                                       EF.Functions.ILike(DateFunctions.ToChar(b.Attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue), "YYYY-MM-DD"), $"%{search}%"));
        }

        var overtimeLetters = await queries.ToListAsync();

        var overtimeLetterList = overtimeLetters.Select(x => new OvertimeLetterDetailReportData
        {
            NIK = x.Attendance.EmployeeCode,
            EmployeeName = x.Attendance.EmployeeName,
            CompanyName = x.Attendance.CompanyName,
            OrganizationName = x.Attendance.OrganizationName,
            PositionName = x.Attendance.PositionName,
            TitleName = x.Attendance.TitleName,
            DateSubmission = x.Attendance.AttendanceDate,
            TimeIn = x.Attendance.In,
            TimeOut = x.Attendance.Out,
            OvertimeIn = x.Attendance.OvertimeIn ?? TimeOnly.MinValue,
            OvertimeOut = x.Attendance.OvertimeOut ?? TimeOnly.MinValue,
            RealOvertime = x.Attendance.RealOvertime ?? TimeOnly.MinValue,
            AccumlativeOvertime = x.Attendance.AccumlativeOvertime ?? TimeOnly.MinValue
        }).ToList();

        return new PaginatedList<OvertimeLetterDetailReportData>(
            overtimeLetterList,
            overtimeLetterList.Count,
            paginationConfig.PageNumber,
            paginationConfig.PageSize);
    }

    private async Task<CutOffListItem> GetCutOffConfiguration(Guid companyKey, int year)
    {
        var cutoffResult = await _mediator.Send(new GetCutOffsQuery([c => c.CompanyKey == companyKey &&
                                                                          c.YearPeriod == year]));

        return cutoffResult.CutOffs.FirstOrDefault() ??
            throw new ArgumentException("CutOff configuration not found for specified company and year");
    }

    private double ParseWorkingHour(TimeOnly workingHour)
    {
        return workingHour.Hour + (workingHour.Minute / 60.0);
    }
}
