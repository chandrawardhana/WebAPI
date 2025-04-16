namespace Saga.Domain.Enums
{
    public enum MaritalStatus
    {
        [Display(Name = "Single")]
        Single = 1,
        [Display(Name = "Marriage")]
        Marriage = 2,
        [Display(Name = "Divorced")]
        Divorced = 3,
        [Display(Name = "Widowed")]
        Widowed = 4,
        [Display(Name = "Separated")]
        Separated = 5
    }
}
