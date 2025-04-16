namespace Saga.Domain.Enums
{
    public enum TransferStatus
    {
        [Display(Name = "Submitted")]
        Submitted = 1,
        [Display(Name = "Transferred")]
        Transferred = 2,
        [Display(Name = "Canceled")]
        Canceled = 3,
        [Display(Name = "Draft")]
        Draft = 4,
    }
}
