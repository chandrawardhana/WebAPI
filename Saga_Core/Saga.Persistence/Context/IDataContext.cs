using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Saga.Domain.Entities.Attendance;
using Saga.Domain.Entities.Employees;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.Entities.Payrolls;
using Saga.Domain.Entities.Programs;
using Saga.Domain.Entities.Systems;

namespace Saga.Persistence.Context;

public interface IDataContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    DatabaseFacade Database { get; }

    #region DataSet Organization
    DbSet<Asset> Assets { get; }
    DbSet<Bank> Banks { get; }
    DbSet<Branch> Branches { get; }
    DbSet<City> Cities { get; }
    DbSet<Company> Companies { get; }
    DbSet<CompanyPolicy> CompanyPolicies { get; }
    DbSet<Country> Countries { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<Grade> Grades { get; }
    DbSet<Organization> Organizations { get; }
    DbSet<Position> Positions { get; }
    DbSet<Province> Provinces { get; }
    DbSet<Title> Titles { get; }
    DbSet<TitleQualification> TitleQualifications { get; }
    #endregion

    #region DataSet Employee
    DbSet<Education> Educations { get; }
    DbSet<Employee> Employees { get; }
    DbSet<EmployeeAttendance> EmployeesAttendances { get; }
    DbSet<EmployeeAttendanceDetail> EmployeeAttendanceDetails { get; }
    DbSet<EmployeeEducation> EmployeeEducations { get; }
    DbSet<EmployeeExperience> EmployeeExperiences { get; }
    DbSet<EmployeeFamily> EmployeeFamilies { get; }
    DbSet<EmployeeHobby> EmployeeHobbies { get; }
    DbSet<EmployeeLanguage> EmployeeLanguages { get; }
    DbSet<EmployeePayroll> EmployeePayrolls { get; }
    DbSet<EmployeePersonal> EmployeePersonals { get; }
    DbSet<EmployeeSkill> EmployeeSkills { get; }
    DbSet<EmployeeTransfer> EmployeeTransfers { get; }
    DbSet<Ethnic> Ethnics { get; }
    DbSet<Hobby> Hobbies { get; }
    DbSet<Language> Languages { get; }
    DbSet<Nationality> Nationalities { get; }
    DbSet<Religion> Religions { get; }
    DbSet<Skill> Skills { get; }
    #endregion

    #region DataSet System
    DbSet<UserProfile> UserProfile { get; }
    DbSet<NavigationAccess> NavigationAccess { get; }
    DbSet<NavigationAccessDetail> NavigationAccessDetail { get; }
    DbSet<OrganizationAccess> OrganizationAccess { get; }
    #endregion

    #region "DataSet Attendance"
    DbSet<ApprovalTransaction> ApprovalTransactions { get; }
    DbSet<ApprovalConfig> ApprovalConfigs { get; }
    DbSet<ApprovalStamp> ApprovalStamps { get; }
    DbSet<Approver> Approvers { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<AttendanceLogMachine> AttendanceLogMachines { get; }
    DbSet<AttendancePoint> AttendancePoints { get; }
    DbSet<AttendancePointApp> AttendancePointApps { get; }
    DbSet<CutOff> CutOffs { get; }
    DbSet<EarlyOutPermit> EarlyOutPermits { get; }
    DbSet<FingerPrint> FingerPrints { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<LatePermit> LatePermits { get; }
    DbSet<Leave> Leaves { get; }
    DbSet<LeaveSubmission> LeaveSubmissions { get; }
    DbSet<OutPermit> OutPermits { get; }
    DbSet<OvertimeLetter> OvertimeLetters { get; }
    DbSet<OvertimeRate> OvertimeRates { get; }
    DbSet<OvertimeRateDetail> OvertimeRateDetails { get; }
    DbSet<Shift> Shifts { get; }
    DbSet<ShiftDetail> ShiftDetails { get; }
    DbSet<ShiftScheduleDetail> ShiftScheduleDetails { get; }
    DbSet<ShiftSchedule> ShiftSchedules { get; }
    DbSet<StandardWorking> StandardWorkings { get; }
    #endregion

    #region "Program"
    DbSet<EmployeeHierarchy> EmployeeHierarchies { get; }
    DbSet<OrganizationHierarchy> OrganizationHierarchies { get; }
    DbSet<ApprovalRequest> ApprovalRequests { get; }
    #endregion

    #region Payroll
    DbSet<Allowance> Allowance { get; }
    DbSet<AllowanceSub> AllowanceSub { get; }
    DbSet<BpjsConfig> BpjsConfig { get; }
    DbSet<BpjsSubConfig> BpjsSubConfig { get; }
    DbSet<AverageEffectiveRate> AverageEffectiveRate { get; }
    DbSet<AverageEffectiveRateDetail> AverageEffectiveRateDetail { get; }
    DbSet<PayrollTaxConfig> PayrollTaxConfig { get; }
    DbSet<PayslipTemplate> PayslipTemplate { get; }
    DbSet<PayslipTemplateDetail> PayslipTemplateDetail { get; }

    #endregion
}
