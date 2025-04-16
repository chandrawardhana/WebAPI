using MediatR;
using Microsoft.EntityFrameworkCore;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Enums;
using Saga.DomainShared;
using Saga.DomainShared.Interfaces;
using Saga.Mediator.Attendances.AttendanceLogMachineMediator;
using Saga.Mediator.Attendances.AttendancePointAppMediator;
using Saga.Mediator.Attendances.EarlyOutPermitMediator;
using Saga.Mediator.Attendances.HolidayMediator;
using Saga.Mediator.Attendances.LatePermitMediator;
using Saga.Mediator.Attendances.LeaveMediator;
using Saga.Mediator.Attendances.LeaveSubmissionMediator;
using Saga.Mediator.Attendances.OutPermitMediator;
using Saga.Mediator.Attendances.OvertimeLetterMediator;
using Saga.Mediator.Attendances.OvertimeRateMediator;
using Saga.Mediator.Employees.EmployeeMediator;
using Saga.Persistence.Context;
using System.Linq.Expressions;

namespace Saga.Infrastructure.Services;

public class AttendanceService(IDataContext _context,
                               IMediator _mediator) : IAttendanceService
{
    private List<Holiday> masterHolidays { get; set; } = [];
    private IEnumerable<Leave> masterLeaves { get; set; } = [];
    private IEnumerable<OvertimeRate> masterOvertimeRates { get; set; } = [];
    private IEnumerable<EmployeeAttendance> masterEmployeeAttendances { get; set; } = [];
    private IEnumerable<LeaveSubmission> transLeaves { get; set; } = [];
    private IEnumerable<LatePermit> transLateSubmissions { get; set; } = [];
    private IEnumerable<OutPermit> transOutPermits { get; set; } = [];
    private IEnumerable<EarlyOutPermit> transEarlyOutPermits { get; set; } = [];
    private IEnumerable<OvertimeLetter> transOvertimeLetters { get; set; } = [];
    private IEnumerable<AttendancePointApp> appAttendances { get; set; } = [];
    private IEnumerable<AttendanceLogMachine> fingerPrintAttendances { get; set; } = [];
    private Leave[]? leaves { get; set; } = [];
    private ShiftDetail? shiftDetail { get; set; }
    private Holiday? holiday { get; set; }
    private LeaveSubmission? leaveSubmission { get; set; }
    private LatePermit? latePermit { get; set; }
    private EarlyOutPermit? earlyOutPermit { get; set; }
    private OutPermit? outPermit { get; set; }
    private EmployeeAttendance? employeeAttendance { get; set; }
    private OvertimeLetter? overtimeLetter { get; set; }
    private OvertimeRate? overtimeRate { get; set; }
    private int countAlpha = 0;

    public async Task<Result> CalculationAttendanceAsync(Expression<Func<Employee, bool>>[] wheres, (DateOnly StartDate, DateOnly EndDate) dateRange, CancellationToken cancellationToken = default)
    {
        try
        {
            var startDate = dateRange.StartDate;
            var endDate = dateRange.EndDate;

            //Pengambilan data master
            var resultLeaves = await _mediator.Send(new GetLeavesQuery([]));
            masterLeaves = resultLeaves.ToArray();

            var findHolidays = await _mediator.Send(new GetHolidaysQuery([x => x.DateEvent >= startDate]));

            findHolidays.ForEach(x =>
            {
                if (x.Duration == 1)
                {
                    masterHolidays.Add(x);
                }
                else
                {
                    for (var i = 0; i < x.Duration; i++)
                    {
                        masterHolidays.Add(new Holiday
                        {
                            Key = x.Key,
                            Name = x.Name,
                            Duration = 1,
                            Description = x.Description,
                            DateEvent = x.DateEvent.AddDays(i),
                            CompanyKeys = x.CompanyKeys
                        });
                    }
                }
            });

            var resultEmployeeAttendances = await _mediator.Send(new GetEmployeeAttendanceWithDetailsQuery(wheres));
            masterEmployeeAttendances = resultEmployeeAttendances.ToArray();

            var resultOvertimeRates = await _mediator.Send(new GetOvertimeRateWithDetailsQuery([]));
            masterOvertimeRates = resultOvertimeRates.ToArray();

            //Pengambilan data transaksi berdasarkan StartDate EndDate
            var resultTransLeaves = await _mediator.Send(new GetLeaveSubmissionsQuery([x => DateOnly.FromDateTime(x.DateStart) >= startDate && DateOnly.FromDateTime(x.DateEnd) <= endDate && x.ApprovalStatus == ApprovalStatus.Approve]));
            transLeaves = resultTransLeaves.ToArray();

            var resultTransLateSubmissions = await _mediator.Send(new GetLatePermitsQuery([x => DateOnly.FromDateTime(x.DateSubmission) >= startDate && DateOnly.FromDateTime(x.DateSubmission) <= endDate && x.ApprovalStatus == ApprovalStatus.Approve]));
            transLateSubmissions = resultTransLateSubmissions.ToArray();

            var resultTransOutPermits = await _mediator.Send(new GetOutPermitsQuery([x => x.DateSubmission >= startDate && x.DateSubmission <= endDate && x.ApprovalStatus == ApprovalStatus.Approve]));
            transOutPermits = resultTransOutPermits.ToArray();

            var resultTransEarlyOutPermits = await _mediator.Send(new GetEarlyOutPermitsQuery([x => x.DateSubmission >= startDate && x.DateSubmission <= endDate && x.ApprovalStatus == ApprovalStatus.Approve]));
            transEarlyOutPermits = resultTransEarlyOutPermits.ToArray();

            var resultTransOvertimeLetters = await _mediator.Send(new GetOvertimeLettersQuery([x => x.DateSubmission >= startDate && x.DateSubmission <= endDate && x.ApprovalStatus == ApprovalStatus.Approve]));
            transOvertimeLetters = resultTransOvertimeLetters.ToArray();

            var resultAppAttendances = await _mediator.Send(new GetAttendancePointAppsQuery([x => DateOnly.FromDateTime(x.AbsenceTime) >= startDate && DateOnly.FromDateTime(x.AbsenceTime) <= endDate]));
            appAttendances = resultAppAttendances.ToArray();

            var resultAttendanceLogMachines = await _mediator.Send(new GetAttendanceLogMachinesQuery([x => DateOnly.FromDateTime(x.LogTime) >= startDate && DateOnly.FromDateTime(x.LogTime) <= endDate]));
            fingerPrintAttendances = resultAttendanceLogMachines.ToArray();


            var employees = await _mediator.Send(new GetEmployeesQuery(wheres));
            foreach (var employee in employees.Employees)
            {
                var currentDate = startDate;
                while (currentDate <= endDate)
                {
                    await ProcessEmployeeAttendance(employee, currentDate);

                    currentDate = currentDate.AddDays(1);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new[] { $"Error saving attendance: {ex.Message}" });
        }
    }

    private async Task ProcessEmployeeAttendance(Employee employee, DateOnly currentDate)
    {
        leaves = masterLeaves.Where(x => x.CompanyKey == employee.CompanyKey).ToArray();

        leaveSubmission = transLeaves?.FirstOrDefault(x =>  x.EmployeeKey == employee.Key &&
                                                            currentDate >= DateOnly.FromDateTime(x.DateStart) &&
                                                            currentDate <= DateOnly.FromDateTime(x.DateEnd)
                                                      );

        latePermit = transLateSubmissions?.FirstOrDefault(x => x.EmployeeKey == employee.Key && DateOnly.FromDateTime(x.DateSubmission) == currentDate);

        earlyOutPermit = transEarlyOutPermits?.FirstOrDefault(x => x.EmployeeKey == employee.Key && x.DateSubmission == currentDate);
        outPermit = transOutPermits?.FirstOrDefault(x => x.EmployeeKey == employee.Key && x.DateSubmission == currentDate);
        overtimeLetter = transOvertimeLetters?.FirstOrDefault(x => x.EmployeeKey == employee.Key && x.DateSubmission == currentDate);

        var findAppAttendances = appAttendances.Where(x => x.EmployeeKey == employee.Key).ToArray();

        var findFingerPrintAttendances = fingerPrintAttendances.Where(x => x.EmployeeKey == employee.Key).ToArray();

        employeeAttendance = masterEmployeeAttendances.FirstOrDefault(x => x.EmployeeKey == employee.Key);

        holiday = masterHolidays.Where(x => x.DateEvent == currentDate && x.CompanyKeys.Contains(employee.CompanyKey)).FirstOrDefault();

        overtimeRate = masterOvertimeRates.Where(x => x.CompanyKey == employee.CompanyKey).FirstOrDefault();

        var employeeShift = employeeAttendance?.ShiftSchedule?.ShiftScheduleDetails?.FirstOrDefault(x => x.Date == currentDate);

        if (employeeShift.ShiftDetailKey == Guid.Empty)
        {
            // If shift detail key is empty, get the shift detail from employee's shift based on the current day of week
            var dayOfWeek = currentDate.DayOfWeek;
            var mappedDay = MapDayOfWeekToCustomDay(dayOfWeek);

            // Get shift detail from employee's assigned shift based on the day of week
            shiftDetail = employeeAttendance?.Shift.ShiftDetails?.FirstOrDefault(x => x.Day == mappedDay);
        }
        else
        {
            //Use the shift detail from shift schedule
            shiftDetail = employeeShift.ShiftDetail;
        }

        var shiftName = String.Empty;
        var shiftDetailsByName = employeeAttendance.Shift.ShiftDetails
                                              .Where(sd => sd != null && sd.DeletedAt == null)
                                              .GroupBy(sd => sd.WorkName)
                                              .ToDictionary(g => g.Key, g => g.First());

        // Find the start of this work week (Monday to Friday for calculation)
        var weekStartDate = currentDate.AddDays(-5);

        // Calculate weekly work hours
        double weeklyHours = employeeAttendance.ShiftSchedule.ShiftScheduleDetails
                                                  .Where(ss => ss.Date >= weekStartDate && ss.Date < currentDate && ss.ShiftName != "OFF")
                                                  .Select(ss => CalculateShiftHours(ss.ShiftName, shiftDetailsByName))
                                                  .Sum();

        // Calculate how many hours are needed to reach 40 hours
        double hoursNeeded = 40 - weeklyHours;

        if (employeeShift != null) //Check if there's a specific entry in ShiftScheduleDetail for this date
        {
            if (shiftDetail?.WorkName == "OFF" && hoursNeeded > 0) // If shift detail off but totalhours less than 40 hours
            {
                var saturdayShift = shiftDetailsByName.Values
                    .FirstOrDefault(sd => sd.Day == Day.Saturday);

                if (saturdayShift != null)
                    shiftName = saturdayShift.WorkName;
            }
            else
            {
                shiftName = $"SHIFT {currentDate.Day}";
            }
        }
        else
        {
            if (shiftDetail != null)
                shiftName = shiftDetail.WorkName;
        }


        var attendance = new Attendance
        {
            Key = Guid.NewGuid(),
            EmployeeKey = employee.Key,
            EmployeeCode = employee.Code,
            EmployeeName = (employee.FirstName ?? String.Empty) + " " + (employee.LastName ?? String.Empty),
            CompanyKey = employee.CompanyKey,
            CompanyName = employee.Company.Name,
            OrganizationKey = employee.OrganizationKey,
            OrganizationName = employee.Organization.Name,
            PositionKey = employee.PositionKey,
            PositionName = employee.Position.Name,
            TitleKey = employee.TitleKey,
            TitleName = employee.Title.Name,
            AttendanceDate = currentDate,
            AttendanceDay = currentDate.DayOfWeek.ToString(),
            ShiftName = shiftName,
            IsMobileApp = false,
            IsFingerPrintMachine = false,
            Latitude = null,
            Longitude = null
        };

        var appAttendancesCheckIn = findAppAttendances.FirstOrDefault(x => x.InOutMode == InOutMode.CheckIn &&
                                                                       DateOnly.FromDateTime(x.AbsenceTime) == currentDate);
        var fingerPrintAttendancesCheckIn = findFingerPrintAttendances.Where(x => x.InOutMode == InOutMode.CheckIn &&
                                                                              DateOnly.FromDateTime(x.LogTime) == currentDate)
                                                                  .OrderByDescending(x => x.LogTime)
                                                                  .FirstOrDefault();

        attendance.In = await ProcessCheckInTime(attendance, appAttendancesCheckIn, fingerPrintAttendancesCheckIn);

        if (appAttendancesCheckIn != null)
        {
            attendance.IsMobileApp = true;
            attendance.Latitude = appAttendancesCheckIn.Latitude;
            attendance.Longitude = appAttendancesCheckIn.Longitude;
        }
        else if (fingerPrintAttendancesCheckIn != null)
        {
            attendance.IsFingerPrintMachine = true;
        }

        var appAttendancesCheckOut = findAppAttendances.FirstOrDefault(x => x.InOutMode == InOutMode.CheckOut &&
                                                                        DateOnly.FromDateTime(x.AbsenceTime) == currentDate);
        var fingerPrintAttendancesCheckOut = findFingerPrintAttendances
                                            .Where(x => x.InOutMode == InOutMode.CheckOut)
                                            .OrderBy(x => x.LogTime)
                                            .FirstOrDefault();

        attendance.Out = await ProcessCheckOutTime(attendance, appAttendancesCheckOut, fingerPrintAttendancesCheckOut);

        if (appAttendancesCheckOut != null)
        {
            attendance.IsMobileApp = true;
            attendance.Latitude = appAttendancesCheckOut.Latitude;
            attendance.Longitude = appAttendancesCheckOut.Longitude;
        }
        else if (fingerPrintAttendancesCheckOut != null)
        {
            attendance.IsFingerPrintMachine = true;
        }

        // Determine attendance status based on rules
        var hasAttendanceRecord = appAttendancesCheckIn != null || fingerPrintAttendancesCheckIn != null || appAttendancesCheckOut != null || fingerPrintAttendancesCheckOut != null;
        attendance.Status = DetermineAttendanceStatus(attendance, hasAttendanceRecord);

        attendance.Description = GenerateAttendanceDescription();

        //Calculate TotalLate
        TimeOnly timeIn = attendance.In;

        TimeOnly shiftInTime;
        if (!TimeOnly.TryParse(shiftDetail.In, out shiftInTime))
            shiftInTime = TimeOnly.MinValue;

        //Untuk cek di late detail report semisal belum ada pengajuan ijin terlambat (sudah termasuk toleransi terlambat)
        attendance.ShiftInTime = shiftInTime.AddMinutes(shiftDetail.LateTolerance);

        int totalLateMinutes = 0;
        if (timeIn > shiftInTime)
            totalLateMinutes = (int)(timeIn - shiftInTime).TotalMinutes;

        if (totalLateMinutes > 0)
        {
            int hours = totalLateMinutes / 60;
            int minutes = totalLateMinutes % 60;
            attendance.TotalLate = new TimeOnly(hours % 24, minutes);
        }
        else
        {
            attendance.TotalLate = null;
        }

        //Check for late permits
        bool hasLatePermit = latePermit != null;

        //Get all shift details for the current day
        var dayShiftDetails = employeeAttendance?.Shift?.ShiftDetails?
                                                 .Where(x => x.Day == shiftDetail.Day)
                                                 .ToList() ?? new List<ShiftDetail>();

        //Calculate normal hour and workingHour
        double normalHours = CalculateWorkingHoursForDay(dayShiftDetails);
        double workingHours = normalHours;

        if (hasLatePermit)
        {
            attendance.IsLatePermit = hasLatePermit;
            attendance.TimeIn = latePermit?.TimeIn;
            attendance.IsLateDocument = latePermit != null && latePermit.Documents.Any();
            attendance.LatePermitReason = latePermit?.Description ?? String.Empty;

            double totalLateHours = totalLateMinutes / 60.0;
            workingHours = Math.Max(normalHours - totalLateHours, 0);
        } else if (timeIn > shiftInTime && !hasLatePermit)
        {
            double totalLateHours = totalLateMinutes / 60.0;
            workingHours = Math.Max(normalHours - totalLateHours, 0);
        }

        attendance.NormalHour = FormatWorkingHours(normalHours);
        attendance.WorkingHour = FormatWorkingHours(workingHours);

        //Check for leave submissions
        bool hasLeaveSubmission = leaveSubmission != null;
        if (hasLeaveSubmission)
        {
            var leave = leaves?.FirstOrDefault(x => x.Key == leaveSubmission.LeaveKey);

            attendance.IsLeaveSubmission = hasLeaveSubmission;
            attendance.LeaveKey = leaveSubmission?.LeaveKey;
            attendance.LeaveCode = leaveSubmission?.LeaveCode ?? String.Empty;
            attendance.LeaveName = leave?.Name ?? String.Empty;
            attendance.LeaveDateStart = leaveSubmission?.DateStart;
            attendance.LeaveDateEnd = leaveSubmission?.DateEnd;
            attendance.LeaveDescription = leaveSubmission?.Description;
        }

        //Check for early out permits
        bool hasEarlyOutPermit = earlyOutPermit != null;
        if (hasEarlyOutPermit)
        {
            attendance.IsEarlyOutPermit = hasEarlyOutPermit;
            attendance.EarlyOutPermitTimeOut = earlyOutPermit?.TimeOut;
            attendance.EarlyOutReason = earlyOutPermit?.Description ?? String.Empty;
        }

        //Check for our permits
        bool hasOutPermit = outPermit != null;
        if (hasOutPermit)
        {
            attendance.IsOutPermit = hasOutPermit;
            attendance.OutPermitTimeOut = outPermit?.OutPermitSubmission;
            attendance.OutPermitBackToOffice = outPermit?.BackToWork;
            attendance.OutPermitReason = outPermit?.Description ?? String.Empty;
        }

        //Check for overtime letter
        bool hasOvertimeLetter = overtimeLetter != null;
        if (hasOvertimeLetter)
        {
            attendance.IsOvertimeLetter = hasOvertimeLetter;
            attendance.OvertimeIn = overtimeLetter?.OvertimeIn;
            attendance.OvertimeOut = overtimeLetter?.OvertimeOut;
            var realOvertime = CalculateTimeDifference(overtimeLetter.OvertimeIn, overtimeLetter.OvertimeOut);
            var accumulativeOvertime = CalculateMultipliedOvertime(realOvertime);
            attendance.RealOvertime = TimeOnly.FromTimeSpan(realOvertime);
            attendance.AccumlativeOvertime = TimeOnly.FromTimeSpan(accumulativeOvertime);
        }

        bool isHoliday = holiday != null;
        attendance.AttendanceCode = DetermineAttendanceCode(attendance);

        if (attendance.Status == AttendanceStatus.NotPresent && leaveSubmission == null && !isHoliday)
        {
            //countAlpha++;
            attendance.IsAlpha = true;
            attendance.CountAlpha = 1;
        }

        // hapus terlebih dahulu apabila ada record yg attendance statusnya not present dan ada pengajuan cuti yg belum di approve untuk kemudian ditimpa dengan data baru
        _context.Attendances.Where(x => x.EmployeeKey == employee.Key && x.AttendanceDate == currentDate).ExecuteDelete();

        _context.Attendances.Add(attendance);
    }

    private async Task<TimeOnly> ProcessCheckInTime(Attendance attendance,
                                                  AttendancePointApp? appAttendance,
                                                  AttendanceLogMachine? fingerPrintAttendance)

    {
        TimeOnly time;

        if (appAttendance != null && appAttendance.InOutMode == InOutMode.CheckIn)
        {
            time = TimeOnly.FromDateTime(appAttendance.AbsenceTime);
        }
        else if (fingerPrintAttendance != null && fingerPrintAttendance.InOutMode == InOutMode.CheckIn)
        {
            time = TimeOnly.FromDateTime(fingerPrintAttendance.LogTime);
        }
        else
        {
            time = TimeOnly.MinValue;
        }

        attendance.In = time;
        return await Task.FromResult(time);
    }

    private async Task<TimeOnly> ProcessCheckOutTime(Attendance attendance,
                                                   AttendancePointApp? appAttendance,
                                                   AttendanceLogMachine? fingerPrintAttendance)
    {
        TimeOnly time;

        if (appAttendance != null && appAttendance.InOutMode == InOutMode.CheckOut)
        {
            time = TimeOnly.FromDateTime(appAttendance.AbsenceTime);
        }
        else if (fingerPrintAttendance != null && fingerPrintAttendance.InOutMode == InOutMode.CheckOut)
        {
            time = TimeOnly.FromDateTime(fingerPrintAttendance.LogTime);
        }
        else
        {
            time = TimeOnly.MinValue;
        }

        attendance.Out = time;
        return await Task.FromResult(time);
    }

    private AttendanceStatus DetermineAttendanceStatus(Attendance attendance,
                                                       bool hasAttendanceRecord)
    {
        //Check if it's a day off
        if (shiftDetail.WorkType == WorkType.Off)
            return AttendanceStatus.OffSchedule;

        //Check for leaveSubmission
        if (leaveSubmission != null && leaveSubmission.ApprovalStatus == ApprovalStatus.Approve)
        {
            var leave = leaves?.FirstOrDefault(x => x.Key == leaveSubmission.LeaveKey);

            //Untuk cuti duka
            if (leave != null && leave.Code == "CTD")
            {
                var submissionDate = DateOnly.FromDateTime(leaveSubmission.DateStart);
                var endDate = DateOnly.FromDateTime(leaveSubmission.DateEnd);
                var maxSubmissionDate = endDate.AddDays(2);

                if (submissionDate <= maxSubmissionDate)
                    return AttendanceStatus.Present;

                return AttendanceStatus.NotPresent;
            }

            //Untuk cuti tahunan dan cuti lainnya
            var currentDate = attendance.AttendanceDate;
            var leaveSubmissionStartDate = DateOnly.FromDateTime(leaveSubmission.DateStart);
            var leaveSubmissionEndDate = DateOnly.FromDateTime(leaveSubmission.DateEnd);

            // Check submission window
            var leaveMinSubmissionDate = leaveSubmissionStartDate.AddDays(leave.MinSubmission);
            var leaveMaxSubmissionDate = leaveSubmissionStartDate.AddDays(leave.MaxSubmission);

            if (currentDate < leaveMinSubmissionDate || currentDate > leaveMaxSubmissionDate)
                return AttendanceStatus.NotPresent; // Outside submission window

            // Update quota if it's a quota-based leave
            if (employeeAttendance?.EmployeeAttendanceDetails != null &&
                (leave.Code == "CTT" || leave.Code == "CTP" || leave.Code == "CTB")) // Annual, Pass, or Bonus Leave
            {
                var leaveCategory = DetermineLeaveCategory(leave.Code);
                var attendanceDetail = employeeAttendance.EmployeeAttendanceDetails
                    .FirstOrDefault(x => x.Category == leaveCategory);

                if (attendanceDetail != null)
                {
                    attendanceDetail.Used = (attendanceDetail.Used ?? 0) + 1;
                    _context.EmployeeAttendanceDetails.Update(attendanceDetail);
                }
            }

            return AttendanceStatus.Leave;
        }

        //Check for holiday event
        if (holiday != null)
            return AttendanceStatus.Holiday;

        if (!hasAttendanceRecord)
            return AttendanceStatus.NotPresent;

        //Check for late arrival
        var checkInTime = attendance.In;
        var shiftInTime = TimeOnly.Parse(shiftDetail.In);

        if (checkInTime > shiftInTime.AddMinutes(shiftDetail.LateTolerance))
        {
            if (latePermit != null && latePermit.ApprovalStatus == ApprovalStatus.Approve)
                return AttendanceStatus.Late;
        }

        //Check for early departure
        var checkOutTime = attendance.Out;
        var shiftOutTime = TimeOnly.Parse(shiftDetail.Out);

        if (checkOutTime < shiftOutTime)
        {
            if (earlyOutPermit != null && earlyOutPermit.ApprovalStatus == ApprovalStatus.Approve)
                return AttendanceStatus.EarlyOut;

            if (outPermit != null && outPermit.ApprovalStatus == ApprovalStatus.Approve)
                return AttendanceStatus.Out;
        }

        return AttendanceStatus.Present;
    }

    private string GenerateAttendanceDescription()
    {
        var descriptions = new List<string>();

        //1. Shift description for work type off
        if (shiftDetail.WorkType == WorkType.Off)
            descriptions.Add($"Off Day: {shiftDetail.Shift?.Description}");

        //2. Holiday description
        if (holiday != null)
            descriptions.Add($"Holiday: {holiday.Description}");

        //3. Leave submission description
        if (leaveSubmission != null)
            descriptions.Add($"Leave : {leaveSubmission.Description}");

        //4. Late submission description
        if (latePermit != null)
            descriptions.Add($"Late Permit: {latePermit.Description}");

        //5. Out permit submission description
        if (outPermit != null)
            descriptions.Add($"Out Permit: {outPermit.Description}");

        //6. Early out permit submission description
        if (earlyOutPermit != null)
            descriptions.Add($"Early Out Permit: {earlyOutPermit.Description}");

        return string.Join("; ", descriptions);
    }

    private bool CheckPendingSubmissions(LeaveSubmission? leaveSubmission,
                                                     LatePermit? latePermit,
                                                     EarlyOutPermit? earlyOutPermit,
                                                     OutPermit? outPermit)
    {
        try
        {
            //Check leave submission 
            if (leaveSubmission != null && leaveSubmission.ApprovalStatus != ApprovalStatus.Approve)
                return true;

            //Check late permit
            if (latePermit != null && latePermit.ApprovalStatus != ApprovalStatus.Approve)
                return true;

            //Check early out permit
            if (earlyOutPermit != null && earlyOutPermit.ApprovalStatus != ApprovalStatus.Approve)
                return true;

            //Check out permit
            if (outPermit != null && outPermit.ApprovalStatus != ApprovalStatus.Approve)
                return true;

            return false;
        }
        catch (Exception)
        {
            // If any query throws "not found" exception, it means there's no submission
            return false;
        }
    }

    private LeaveCategory DetermineLeaveCategory(string leaveCode)
    {
        return leaveCode switch
        {
            "CTT" => LeaveCategory.AnnualLeave,
            "CTP" => LeaveCategory.PassLeave,
            "CTB" => LeaveCategory.BonusLeave,
            _ => LeaveCategory.AnnualLeave // Default case
        };
    }

    private Day MapDayOfWeekToCustomDay(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => Day.Monday,
            DayOfWeek.Tuesday => Day.Tuesday,
            DayOfWeek.Wednesday => Day.Wednesday,
            DayOfWeek.Thursday => Day.Thursday,
            DayOfWeek.Friday => Day.Friday,
            DayOfWeek.Saturday => Day.Saturday,
            DayOfWeek.Sunday => Day.Sunday,
            _ => Day.Unknown
        };
    }

    private double CalculateShiftHours(string shiftName, Dictionary<string, ShiftDetail> shiftDetails)
    {
        if (!shiftDetails.TryGetValue(shiftName, out var shiftDetail))
        {
            return 0; // If shift detail not found, assume 0 hours
        }

        // Calculate hours between In and Out times
        var inTime = TimeOnly.Parse(shiftDetail.In);
        var outTime = TimeOnly.Parse(shiftDetail.Out);

        // Handle next day case
        double hours;
        if (shiftDetail.IsNextDay == true)
        {
            hours = (24 - inTime.Hour) + outTime.Hour;
            // Add minutes
            hours -= inTime.Minute / 60.0;
            hours += outTime.Minute / 60.0;
        }
        else
        {
            hours = outTime.Hour - inTime.Hour;
            hours += (outTime.Minute - inTime.Minute) / 60.0;
        }

        return hours;
    }

    private Double CalculateWorkingHoursForDay(IEnumerable<ShiftDetail> dayShiftDetails)
    {
        if (dayShiftDetails == null || !dayShiftDetails.Any())
            return 0;

        double totalWorkingHours = 0;

        var workShifts = dayShiftDetails.Where(x => x.WorkType != WorkType.Break);
        foreach (var workShift in workShifts)
        {
            TimeSpan shiftIn = TimeSpan.Parse(workShift.In);
            TimeSpan shiftOut = TimeSpan.Parse(workShift.Out);

            double shiftHours = CalculateDuration(shiftIn, shiftOut, workShift.IsNextDay ?? false);

            if (workShift.IsCutBreak == true)
            {
                var breakShifts = dayShiftDetails.Where(x => x.WorkType == WorkType.Break);

                double totalBreakHours = 0;
                foreach (var breakShift in breakShifts)
                {
                    TimeSpan breakIn = TimeSpan.Parse(breakShift.In);
                    TimeSpan breakOut = TimeSpan.Parse(breakShift.Out);
                    totalBreakHours += CalculateDuration(breakIn, breakOut, breakShift.IsNextDay ?? false);
                }

                shiftHours -= totalBreakHours;
            }

            totalWorkingHours += shiftHours;
        }

        return totalWorkingHours;
    }

    private double CalculateDuration(TimeSpan startTime, TimeSpan endTime, bool isNextDay)
    {
        if (isNextDay)
            endTime = endTime.Add(TimeSpan.FromDays(1));

        var duration = endTime - startTime;

        return duration.TotalHours;
    }

    private TimeOnly FormatWorkingHours(double hours)
    {
        // Convert hours to hours and minutes in HH:MM format
        int wholeHours = (int)Math.Floor(hours);
        int minutes = (int)Math.Round((hours - wholeHours) * 60);

        // Handle case where minutes round up to 60
        if (minutes == 60)
        {
            wholeHours++;
            minutes = 0;
        }

        // Handle hours exceeding 24 by taking modulo
        wholeHours %= 24;

        return new TimeOnly(wholeHours, minutes);
    }

    private TimeSpan CalculateTimeDifference(TimeOnly startTime, TimeOnly endTime)
    {
        // Handle cases where end time is on the next day
        if (endTime < startTime)
        {
            // Add 24 hours (next day)
            return TimeSpan.FromHours(24) - startTime.ToTimeSpan() + endTime.ToTimeSpan();
        }

        return endTime.ToTimeSpan() - startTime.ToTimeSpan();
    }

    private TimeSpan CalculateMultipliedOvertime(TimeSpan actualOvertime)
    {
        if (overtimeRate?.OvertimeRateDetails == null || !overtimeRate.OvertimeRateDetails.Any())
        {
            // If no rate details found, return actual overtime without multiplier
            return actualOvertime;
        }

        // Sort rate details by level
        var sortedRateDetails = overtimeRate?.OvertimeRateDetails.OrderBy(r => r.Level).ToList();

        double totalHours = actualOvertime.TotalHours;
        double multipliedMinutes = 0;
        double remainingHours = totalHours;

        foreach (var rateDetail in sortedRateDetails)
        {
            if (remainingHours <= 0)
                break;

            // Determine how many hours to apply at this rate
            double hoursAtThisRate = Math.Min(remainingHours, rateDetail.Hours);

            // Apply multiplier
            multipliedMinutes += hoursAtThisRate * 60 * rateDetail.Multiply;

            // Reduce remaining hours
            remainingHours -= hoursAtThisRate;
        }

        // If there are still remaining hours, apply the highest multiplier
        if (remainingHours > 0 && sortedRateDetails.Any())
        {
            var highestRate = sortedRateDetails.Last();
            multipliedMinutes += remainingHours * 60 * highestRate.Multiply;
        }

        // Convert back to TimeSpan
        return TimeSpan.FromMinutes(multipliedMinutes);
    }

    private string DetermineAttendanceCode(Attendance? attendance)
    {
        switch (attendance.Status)
        {
            case AttendanceStatus.Holiday:
                return ":::";
            case AttendanceStatus.Leave:
                return leaveSubmission.LeaveCode ?? "L";
            case AttendanceStatus.Present:
                return ".";
            case AttendanceStatus.NotPresent:
                return "A";
            case AttendanceStatus.OffSchedule:
                return ":::";
            default:
                return ".";
        }
    }
}
