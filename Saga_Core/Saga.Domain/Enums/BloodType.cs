namespace Saga.Domain.Enums
{
    public enum BloodType
    {
        [Display(Name = "A+")]
        A_Positive = 1,
        [Display(Name = "A-")]
        A_Negative = 2,
        [Display(Name = "B+")]
        B_Positive = 3,
        [Display(Name = "B-")]
        B_Negative = 4,
        [Display(Name = "AB+")]
        AB_Positive = 5,
        [Display(Name = "AB-")]
        AB_Negative = 6,
        [Display(Name = "O+")] 
        O_Positive = 7,
        [Display(Name = "O-")]
        O_Negative = 8
    }
}
