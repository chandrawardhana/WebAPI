namespace Saga.Domain.Enums
{
    public enum EmployeeStatus
    {
        [Display(Name = "Probation")]
        Probation = 1,
        [Display(Name = "Contract")]
        Contract = 2,
        [Display(Name = "Permanent")]
        Permanent = 3,
        [Display(Name = "NotActive")]
        NotActive = 4,
        [Display(Name = "Resign")]
        Resign = 5,
    }
}
