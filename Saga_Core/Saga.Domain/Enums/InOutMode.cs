namespace Saga.Domain.Enums
{
    public enum InOutMode
    {
        [Display(Name = "Check-In")]
        CheckIn = 0,
        [Display(Name = "Check-Out")]
        CheckOut = 1,
        [Display(Name = "Break-Out")]
        BreakOut = 2,
        [Display(Name = "Break-In")]
        BreakIn = 3,
        [Display(Name = "OT-In")]
        OTIn = 4,
        [Display(Name = "OT-Out")]
        OTOut = 5
    }
}
