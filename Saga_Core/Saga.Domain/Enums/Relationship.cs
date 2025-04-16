using System.ComponentModel;

namespace Saga.Domain.Enums
{
    public enum Relationship
    {
        [Display(Name = "Father")]
        Father = 1,
        [Display(Name = "Mother")]
        Mother = 2,
        [Display(Name = "Sister")]
        Sister = 3,
        [Display(Name = "Brother")]
        Brother = 4,
        [Display(Name = "GrandFather")]
        Grandfather = 5,
        [Display(Name = "GrandMother")]
        Grandmother = 6,
        [Display(Name = "Uncle")]
        Uncle = 7,
        [Display(Name = "Aunt")]
        Aunt = 8,
        [Display(Name = "Nephew")]
        Nephew = 9
    }
}
