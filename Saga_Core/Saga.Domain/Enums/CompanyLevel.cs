namespace Saga.Domain.Enums
{
    public enum CompanyLevel
    {
        [Display(Name = "Holding")]
        Holding = 1, // example Sido Agung Group; Jakarta
        [Display(Name = "SubHolding")]
        SubHolding = 2, // example BMR, PIP
        [Display(Name = "Unit")]
        Unit = 3, // example Yogya, Temanggung, Semarang under BMR; Garut, Sumedang, Tasik under PIP
        [Display(Name = "SubUnit")]
        SubUnit = 4 // example Subang under Sumedang under PIP
    }
}
