namespace Saga.Domain.Enums
{
    public enum ConnectionMethod
    {
        [Display(Name = "Web")]
        Web = 1,
        [Display(Name = "Port")]
        Port = 2,
        [Display(Name = "Serial")] 
        Serial = 3
    }
}
