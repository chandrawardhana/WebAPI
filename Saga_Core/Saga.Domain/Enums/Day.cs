namespace Saga.Domain.Enums
{
    public enum Day
    {
        Unknown,
        [Display(Name = "Every Day")]
        EveryDay = 1,
        [Display(Name = "Monday")]
        Monday = 2,
        [Display(Name = "Tuesday")] 
        Tuesday = 3,
        [Display(Name = "Wednesday")]
        Wednesday = 4,
        [Display(Name = "Thursday")]
        Thursday = 5,
        [Display(Name = "Friday")]
        Friday = 6,
        [Display(Name = "Saturday")]
        Saturday = 7,
        [Display(Name = "Sunday")]
        Sunday = 8,
        [Display(Name = "Holiday")]
        Holiday = 9
    }
}
