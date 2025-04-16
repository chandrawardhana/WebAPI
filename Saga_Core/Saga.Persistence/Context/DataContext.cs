using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Saga.Domain.Abstracts;
using Saga.Persistence.Extensions;
using System.Reflection;
using System.Text.Json;
using Saga.Domain.Entities.Organizations;
using Saga.Domain.Entities.Systems;
using Saga.Domain.Entities.Employees;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Saga.Persistence.Models;
using Saga.Domain.Entities.Attendance;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection.Emit;
using Saga.Domain.Entities.Programs;
using Saga.Domain.Entities.Payrolls;
using Saga.Domain.Enums;

namespace Saga.Persistence.Context;

public class DataContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IDataContext
//public class DataContext : DbContext, IDataContext
{
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;
    public DataContext(
        DbContextOptions<DataContext> options, 
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor
    ) : base(options)
    {
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }

    private static ValueConverter OwnConvert<T>()
    {
        JsonSerializerOptions option = new()
        {
            PropertyNameCaseInsensitive = true
        };

        return new ValueConverter<T, string>(
            x => JsonSerializer.Serialize(x, option),
            x => JsonSerializer.Deserialize<T>(x, option)
            //v => JsonConvert.SerializeObject(v),
            //v => JsonConvert.DeserializeObject<T>(v)
        );
    }

    private static ValueConverter DateTimeConvert()
        => new ValueConverter<DateTime, DateTime>(
            x => x.ToUniversalTime(),
            x => x.ToLocalTime()
        );

    private static ValueConverter GuidArrayConvert()
        => new ValueConverter<Guid[], string>(
            x => x == null || !x.Any()
                ? string.Empty
                : string.Join(",", x.Where(g => g != Guid.Empty)),
            x => string.IsNullOrWhiteSpace(x)
                ? Array.Empty<Guid>()
                : x.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(s => ParseGuidSafely(s.Trim()))
                   .Where(g => g != Guid.Empty)
                   .ToArray()
        );

    private static ValueConverter ListArrayConvert()
        => new ValueConverter<List<Guid>, string>(
            x => x == null || !x.Any()
                ? string.Empty
                : string.Join(",", x.Where(g => g != Guid.Empty)),
            x => string.IsNullOrWhiteSpace(x)
                ? new List<Guid>()
                : x.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(s => ParseGuidSafely(s.Trim()))
                   .Where(g => g != Guid.Empty)
                   .ToList()
        );

    private static Guid ParseGuidSafely(string guidString)
    {
        if (string.IsNullOrWhiteSpace(guidString))
            return Guid.Empty;

        return Guid.TryParse(guidString, out Guid result)
            ? result
            : Guid.Empty;
    }

    private static ValueConverter TimeSpanArrayConvert()
        => new ValueConverter<TimeSpan[], string>(
            x => x == null || !x.Any()
                ? string.Empty
                : string.Join(",", x.Where(t => t != TimeSpan.Zero)
                    .Select(t => t.ToString("hh\\:mm"))),
            x => string.IsNullOrWhiteSpace(x)
                ? Array.Empty<TimeSpan>()
                : x.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => ParseTimeSpanSafely(s.Trim()))
                    .Where(t => t != TimeSpan.Zero)
                    .ToArray()
        );

    private static TimeSpan ParseTimeSpanSafely(string timeString)
    {
        if (string.IsNullOrWhiteSpace(timeString))
            return TimeSpan.Zero;
        return TimeSpan.TryParse(timeString, out TimeSpan result)
            ? result
            : TimeSpan.Zero;
    }

    private static ValueComparer<T[]> ArrayClassValueComparer<T>() where T : class
        => new ValueComparer<T[]>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            a => a != null ? a.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())) : 0,
            a => a != null ? a.ToArray() : Array.Empty<T>());


    private static ValueComparer<T[]> ArrayValueComparer<T>() where T : struct
        => new ValueComparer<T[]>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            a => a != null ? a.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())) : 0,
            a => a != null ? a.ToArray() : Array.Empty<T>());

    private static ValueComparer<List<T>> ListValueComparer<T>() where T : struct
        => new ValueComparer<List<T>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            a => a != null ? a.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())) : 0,
            a => a != null ? new List<T>(a) : new List<T>());

    private static ValueComparer<TimeSpan[]> ArrayTimeSpanValueComparer()
        => new ValueComparer<TimeSpan[]>(
            (a, b) => (a == null && b == null) ||
                      (a != null && b != null &&
                       a.Length == b.Length &&
                       a.Zip(b, (timeA, timeB) => timeA.Equals(timeB)).All(eq => eq)),
            a => a != null
                ? a.Where(x => x != TimeSpan.Zero)
                   .Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode()))
                : 0,
            a => a != null
                ? a.Where(x => x != TimeSpan.Zero).ToArray()
                : Array.Empty<TimeSpan>()
        );

    private static ValueConverter<TimeOnly, TimeSpan> TimeOnlyConverter()
    => new(
        timeOnly => timeOnly.ToTimeSpan(),
        timeSpan => TimeOnly.FromTimeSpan(timeSpan)
    );

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        IEnumerable<Type> softDeleteEntities = typeof(AuditTrail).Assembly.GetTypes()
                                .Where(t => typeof(AuditTrail).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var softDeleteEntity in softDeleteEntities)
        {
            builder.Entity(softDeleteEntity)
                    .HasQueryFilter(
                        QueryFilterExtension.GenerateQueryFilter(softDeleteEntity)
                    );
        }

        builder.HasDefaultSchema("public");

        // builder.Entity<LogMailSender>().Property(x => x.QueueDate).HasConversion(DateTimeConvert());

        #region "Attendance"
        builder.Entity<ApprovalStamp>().Property(x => x.DateStamp).HasConversion(DateTimeConvert());
        builder.Entity<Attendance>().Property(x => x.In)
                            .HasConversion(TimeOnlyConverter())
                            .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.Out)
                            .HasConversion(TimeOnlyConverter())
                            .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.NormalHour)
                            .HasConversion(TimeOnlyConverter())
                            .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.TotalLate)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.EarlyOutPermitTimeOut)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.OutPermitTimeOut)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.OutPermitBackToOffice)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.OvertimeIn)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.OvertimeOut)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.RealOvertime)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.AccumlativeOvertime)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.WorkingHour)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.TimeIn)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<Attendance>().Property(x => x.ShiftInTime)
                                    .HasConversion(TimeOnlyConverter())
                                    .HasColumnType("time");

        builder.Entity<EarlyOutPermit>().Property(x => x.Documents)
                                      .HasConversion(GuidArrayConvert())
                                      .Metadata.SetValueComparer(ArrayValueComparer<Guid>());

        builder.Entity<FingerPrint>().Property(x => x.RetrieveScheduleTimes)
                                     .HasConversion(TimeSpanArrayConvert())
                                     .Metadata.SetValueComparer(ArrayTimeSpanValueComparer());

        builder.Entity<Holiday>().Property(x => x.CompanyKeys)
                                 .HasConversion(GuidArrayConvert())
                                 .Metadata.SetValueComparer(ArrayValueComparer<Guid>());

        builder.Entity<LatePermit>().Property(x => x.DateSubmission).HasConversion(DateTimeConvert());
        builder.Entity<LatePermit>().Property(x => x.Documents)
                                    .HasConversion(GuidArrayConvert())
                                    .Metadata.SetValueComparer(ArrayValueComparer<Guid>());
        builder.Entity<LeaveSubmission>().Property(x => x.DateStart).HasConversion(DateTimeConvert());
        builder.Entity<LeaveSubmission>().Property(x => x.DateEnd).HasConversion(DateTimeConvert());
        builder.Entity<LeaveSubmission>().Property(x => x.Documents)
                                         .HasConversion(GuidArrayConvert())
                                         .Metadata.SetValueComparer(ArrayValueComparer<Guid>());

        builder.Entity<OutPermit>().Property(x => x.Documents)
                                   .HasConversion(GuidArrayConvert())
                                   .Metadata.SetValueComparer(ArrayValueComparer<Guid>());

        builder.Entity<OvertimeLetter>().Property(x => x.Documents)
                                      .HasConversion(GuidArrayConvert())
                                      .Metadata.SetValueComparer(ArrayValueComparer<Guid>());
        #endregion

        #region "Employee"
        builder.Entity<Employee>().Property(x => x.HireDate).HasConversion(DateTimeConvert());
        builder.Entity<EmployeePayroll>().Property(x => x.TaxRegistered).HasConversion(DateTimeConvert());
        builder.Entity<EmployeePayroll>().Property(x => x.HealthNationalityInsuranceRegistered).HasConversion(DateTimeConvert());
        builder.Entity<EmployeePayroll>().Property(x => x.LaborNationalityInsuranceRegistered).HasConversion(DateTimeConvert());
        builder.Entity<EmployeePayroll>().Property(x => x.PensionNationalityInsuranceRegistered).HasConversion(DateTimeConvert());
        builder.Entity<Employee>().Property(x => x.ResignDate).HasConversion(DateTimeConvert());
        builder.Entity<EmployeeFamily>().Property(x => x.BoD).HasConversion(DateTimeConvert());
        builder.Entity<EmployeePersonal>().Property(x => x.NationalityRegistered).HasConversion(DateTimeConvert());
        builder.Entity<EmployeePersonal>().Property(x => x.DateOfBirth).HasConversion(DateTimeConvert());
        #endregion

        #region "Organization"
        builder.Entity<CompanyPolicy>().Property(x => x.EffectiveDate).HasConversion(DateTimeConvert());
        builder.Entity<CompanyPolicy>().Property(x => x.ExpiredDate).HasConversion(DateTimeConvert());
        builder.Entity<TitleQualification>().Property(x => x.SkillKeys)
                                            .HasConversion(ListArrayConvert())
                                            .Metadata.SetValueComparer(ListValueComparer<Guid>());
        builder.Entity<TitleQualification>().Property(x => x.LanguageKeys)
                                            .HasConversion(ListArrayConvert())
                                            .Metadata.SetValueComparer(ListValueComparer<Guid>());
        #endregion

        #region Payroll
        builder.Entity<AverageEffectiveRate>().Property(x => x.TaxStatuses)
                                                .HasConversion(OwnConvert<TaxStatus[]>())
                                                .Metadata.SetValueComparer(ArrayValueComparer<TaxStatus>());

        builder.Entity<PayrollTaxConfig>().Property(x => x.TaxRate)
                                            .HasConversion(OwnConvert<PayrollTaxRate[]>())
                                            .Metadata.SetValueComparer(ArrayClassValueComparer<PayrollTaxRate>());
        #endregion

        #region "Program"
        builder.Entity<ApprovalRequest>().ToView("mv_approval_request", "Program")
                                         .HasKey(x => x.ApproverEmail);
        builder.Entity<EmployeeHierarchy>().ToView("employee_hierarchy", "Program")
                                           .HasKey(x => x.EmployeeKey);
        builder.Entity<OrganizationHierarchy>().ToView("organization_hierarchy", "Program")
                                               .HasKey(x => new { x.OrgKey, x.IsCompany });
        #endregion

        #region "System"
        builder.Entity<OrganizationAccess>().Property(x => x.AccessDetail)
                                            .HasConversion(GuidArrayConvert())
                                            .Metadata.SetValueComparer(ArrayValueComparer<Guid>());
        #endregion

        builder.AddDateFunctions();

        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableServiceProviderCaching();
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }

    #region DataSet Organization
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyPolicy> CompanyPolicies => Set<CompanyPolicy>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<Title> Titles => Set<Title>();
    public DbSet<TitleQualification> TitleQualifications => Set<TitleQualification>();
    #endregion

    #region DataSet Employee
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeAttendance> EmployeesAttendances => Set<EmployeeAttendance>();
    public DbSet<EmployeeAttendanceDetail> EmployeeAttendanceDetails => Set<EmployeeAttendanceDetail>();
    public DbSet<EmployeeEducation> EmployeeEducations => Set<EmployeeEducation>();
    public DbSet<EmployeeExperience> EmployeeExperiences => Set<EmployeeExperience>();
    public DbSet<EmployeeFamily> EmployeeFamilies => Set<EmployeeFamily>();
    public DbSet<EmployeeHobby> EmployeeHobbies => Set<EmployeeHobby>();
    public DbSet<EmployeeLanguage> EmployeeLanguages => Set<EmployeeLanguage>();
    public DbSet<EmployeePayroll> EmployeePayrolls => Set<EmployeePayroll>();
    public DbSet<EmployeePersonal> EmployeePersonals => Set<EmployeePersonal>();
    public DbSet<EmployeeSkill> EmployeeSkills => Set<EmployeeSkill>();
    public DbSet<EmployeeTransfer> EmployeeTransfers => Set<EmployeeTransfer>();
    public DbSet<Ethnic> Ethnics => Set<Ethnic>();
    public DbSet<Hobby> Hobbies => Set<Hobby>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Nationality> Nationalities => Set<Nationality>();
    public DbSet<Religion> Religions => Set<Religion>();
    public DbSet<Skill> Skills => Set<Skill>();
    #endregion

    #region DataSet System
    public DbSet<UserProfile> UserProfile => Set<UserProfile>();
    public DbSet<NavigationAccess> NavigationAccess => Set<NavigationAccess>();
    public DbSet<NavigationAccessDetail> NavigationAccessDetail => Set<NavigationAccessDetail>();
    public DbSet<OrganizationAccess> OrganizationAccess => Set<OrganizationAccess>();
    #endregion

    #region "DataSet Attendance"
    public DbSet<ApprovalTransaction> ApprovalTransactions => Set<ApprovalTransaction>();
    public DbSet<ApprovalConfig> ApprovalConfigs => Set<ApprovalConfig>();
    public DbSet<ApprovalStamp> ApprovalStamps => Set<ApprovalStamp>();
    public DbSet<Approver> Approvers => Set<Approver>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<AttendanceLogMachine> AttendanceLogMachines => Set<AttendanceLogMachine>();
    public DbSet<AttendancePoint> AttendancePoints => Set<AttendancePoint>();
    public DbSet<AttendancePointApp> AttendancePointApps => Set<AttendancePointApp>();
    public DbSet<CutOff> CutOffs => Set<CutOff>();
    public DbSet<EarlyOutPermit> EarlyOutPermits => Set<EarlyOutPermit>();
    public DbSet<FingerPrint> FingerPrints => Set<FingerPrint>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<LatePermit> LatePermits => Set<LatePermit>();
    public DbSet<Leave> Leaves => Set<Leave>();
    public DbSet<LeaveSubmission> LeaveSubmissions => Set<LeaveSubmission>();
    public DbSet<OutPermit> OutPermits => Set<OutPermit>();
    public DbSet<OvertimeLetter> OvertimeLetters => Set<OvertimeLetter>();
    public DbSet<OvertimeRate> OvertimeRates => Set<OvertimeRate>();
    public DbSet<OvertimeRateDetail> OvertimeRateDetails => Set<OvertimeRateDetail>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftDetail> ShiftDetails => Set<ShiftDetail>();
    public DbSet<ShiftScheduleDetail> ShiftScheduleDetails => Set<ShiftScheduleDetail>();
    public DbSet<ShiftSchedule> ShiftSchedules => Set<ShiftSchedule>();
    public DbSet<StandardWorking> StandardWorkings => Set<StandardWorking>();
    #endregion

    #region "DataSet Program"
    public DbSet<EmployeeHierarchy> EmployeeHierarchies => Set<EmployeeHierarchy>();
    public DbSet<OrganizationHierarchy> OrganizationHierarchies => Set<OrganizationHierarchy>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    #endregion

    #region Payroll
    public DbSet<Allowance> Allowance => Set<Allowance>();
    public DbSet<AllowanceSub> AllowanceSub => Set<AllowanceSub>();
    public DbSet<AverageEffectiveRate> AverageEffectiveRate => Set<AverageEffectiveRate>();
    public DbSet<AverageEffectiveRateDetail> AverageEffectiveRateDetail => Set<AverageEffectiveRateDetail>();
    public DbSet<PayrollTaxConfig> PayrollTaxConfig => Set<PayrollTaxConfig>();
    public DbSet<BpjsConfig> BpjsConfig => Set<BpjsConfig>();
    public DbSet<BpjsSubConfig> BpjsSubConfig => Set<BpjsSubConfig>();
    public DbSet<PayslipTemplate> PayslipTemplate => Set<PayslipTemplate>();
    public DbSet<PayslipTemplateDetail> PayslipTemplateDetail => Set<PayslipTemplateDetail>();
    #endregion
}
