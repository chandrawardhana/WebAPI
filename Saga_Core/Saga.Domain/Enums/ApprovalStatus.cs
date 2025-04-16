namespace Saga.Domain.Enums
{
    public enum ApprovalStatus
    {
        [Display(Name = "New")]
        New = 1,
        [Display(Name = "Waiting")]
        Waiting = 2,
        [Display(Name = "Approve")]
        Approve = 3,
        [Display(Name = "Reject")]
        Reject = 4,
        [Display(Name = "Revision")]
        Revision = 5
    }
}
