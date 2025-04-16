
namespace Saga.Domain.Enums;

public enum BpjsBaseOnCalculation
{
    [Display(Name = "Basic Salary")]
    BasicSalary = 1,
    [Display(Name = "Fixed Allowance")]
    FixedAllowance = 2,
    [Display(Name = "Basic Salary And Fixed Allowance")]
    BasicSalaryAndFixedAllowance = 3,
    [Display(Name = "Fixed")]
    Fixed = 4
}
