namespace Saga.Domain.Enums
{
    public enum ApprovalCategory
    {
        [Display(Name = "Leave Permit")]
        LeavePermit = 1,
        [Display(Name = "Late Permit")]
        LatePermit = 2,
        [Display(Name = "Early Out Permit")]
        EarlyOutPermit = 3,
        [Display(Name = "Out Permit")]
        OutPermit = 4,
        [Display(Name = "Overtime Letter")]
        OvertimeLetter = 5
    }
}
