namespace Saga.Domain.Enums
{
    public enum TransferCategory
    {
        [Display(Name = "Promotion")]
        Promotion = 1,
        [Display(Name = "Demotion")]
        Demotion = 2,
        [Display(Name = "Rotation")]
        Rotation = 3,
        [Display(Name = "Mutation")]
        Mutation = 4
    }
}
