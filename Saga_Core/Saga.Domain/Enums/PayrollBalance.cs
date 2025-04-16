
namespace Saga.Domain.Enums;

public enum PayrollBalance
{
    [Display(Name = "+")]
    Income,
    [Display(Name = "-")]
    Deduction,
    [Display(Name = "T")]
    Balance
}
