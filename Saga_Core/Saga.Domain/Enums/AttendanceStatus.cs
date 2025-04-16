namespace Saga.Domain.Enums
{
    public enum AttendanceStatus
    {
        [Display(Name = "Present")]
        Present = 1,
        [Display(Name = "Not Present")]
        NotPresent = 2,
        [Display(Name = "Leave")]
        Leave = 3,
        [Display(Name = "Late")]
        Late = 4,
        [Display(Name = "Early Out")]
        EarlyOut = 5,
        [Display(Name = "Holiday")]
        Holiday = 6,
        [Display(Name = "OffSchedule")]
        OffSchedule = 7,
        [Display(Name = "Out")]
        Out = 8
    }
}
