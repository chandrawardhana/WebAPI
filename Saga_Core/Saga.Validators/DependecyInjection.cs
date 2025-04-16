
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Saga.Validators.Attendances;
using Saga.Validators.Employees;
using Saga.Validators.Organizations;
using Saga.Validators.Payrolls;

namespace Saga.Validators;

public static class DependecyInjection
{
    public static IServiceCollection AddValidator(this IServiceCollection services)
    {
        #region "Validator Organization"
        services.AddValidatorsFromAssemblyContaining<BankValidator>();
        services.AddValidatorsFromAssemblyContaining<BranchValidator>();
        services.AddValidatorsFromAssemblyContaining<CompanyValidator>();
        services.AddValidatorsFromAssemblyContaining<CompanyPolicyValidator>();
        services.AddValidatorsFromAssemblyContaining<CountryValidator>();
        services.AddValidatorsFromAssemblyContaining<CityValidator>();
        services.AddValidatorsFromAssemblyContaining<CurrencyValidator>();
        services.AddValidatorsFromAssemblyContaining<GradeValidator>();
        services.AddValidatorsFromAssemblyContaining<OrganizationValidator>();
        services.AddValidatorsFromAssemblyContaining<PositionValidator>();
        services.AddValidatorsFromAssemblyContaining<ProvinceValidator>();
        services.AddValidatorsFromAssemblyContaining<TitleValidator>();
        services.AddValidatorsFromAssemblyContaining<TitleQualificationValidator>();
        #endregion

        #region "Validator Employee"
        services.AddValidatorsFromAssemblyContaining<EducationValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeAttendanceDetailValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeAttendanceValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeEducationValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeExperienceValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeFamilyValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeHobbyValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeLanguageValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeePayrollValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeePersonalValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeSkillValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeTransferValidator>();
        services.AddValidatorsFromAssemblyContaining<EmployeeValidator>();
        services.AddValidatorsFromAssemblyContaining<EthnicValidator>();
        services.AddValidatorsFromAssemblyContaining<HobbyValidator>();
        services.AddValidatorsFromAssemblyContaining<LanguageValidator>();
        services.AddValidatorsFromAssemblyContaining<NationalityValidator>();
        services.AddValidatorsFromAssemblyContaining<SkillValidator>();
        services.AddValidatorsFromAssemblyContaining<ReligionValidator>();
        services.AddValidatorsFromAssemblyContaining<CancelEmployeeTransferValidator>();
        #endregion

        #region "Validator Attendance"
        services.AddValidatorsFromAssemblyContaining<ApprovalStampValidator>();
        services.AddValidatorsFromAssemblyContaining<ApprovalTransactionValidator>();
        services.AddValidatorsFromAssemblyContaining<ApprovalConfigValidator>();
        services.AddValidatorsFromAssemblyContaining<ApproverValidator>();
        services.AddValidatorsFromAssemblyContaining<AttendanceValidator>();
        services.AddValidatorsFromAssemblyContaining<AttendancePointValidator>();
        services.AddValidatorsFromAssemblyContaining<AttendancePointAppValidator>();
        services.AddValidatorsFromAssemblyContaining<CutOffValidator>();
        services.AddValidatorsFromAssemblyContaining<EarlyOutPermitValidator>();
        services.AddValidatorsFromAssemblyContaining<FingerPrintValidator>();
        services.AddValidatorsFromAssemblyContaining<HolidayValidator>();
        services.AddValidatorsFromAssemblyContaining<LatePermitValidator>();
        services.AddValidatorsFromAssemblyContaining<LeaveSubmissionValidator>();
        services.AddValidatorsFromAssemblyContaining<LeaveValidator>();
        services.AddValidatorsFromAssemblyContaining<OutPermitValidator>();
        services.AddValidatorsFromAssemblyContaining<OvertimeRateValidator>();
        services.AddValidatorsFromAssemblyContaining<OvertimeRateDetailValidator>();
        services.AddValidatorsFromAssemblyContaining<ShiftDetailValidator>();
        services.AddValidatorsFromAssemblyContaining<ShiftValidator>();
        services.AddValidatorsFromAssemblyContaining<ShiftScheduleDetailValidator>();
        services.AddValidatorsFromAssemblyContaining<ShiftScheduleValidator>();
        services.AddValidatorsFromAssemblyContaining<StandardWorkingValidator>();
        #endregion

        #region Payroll
        services.AddValidatorsFromAssemblyContaining<AverageEffectiveRateValidator>();
        services.AddValidatorsFromAssemblyContaining<PayrollTaxConfigValidator>();
        services.AddValidatorsFromAssemblyContaining<AllowanceValidator>();
        services.AddValidatorsFromAssemblyContaining<AllowanceSubValidator>();
        services.AddValidatorsFromAssemblyContaining<BpjsConfigValidator>();
        services.AddValidatorsFromAssemblyContaining<BpjsSubConfigValidator>();
        services.AddValidatorsFromAssemblyContaining<PayslipTemplateValidator>();
        #endregion

        return services;
    }
}
