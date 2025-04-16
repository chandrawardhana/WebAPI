
using System.ComponentModel.DataAnnotations;

namespace Saga.DomainShared.Models;

public class ApplicationLanguage
{
    [Key]
    public int Id { get; set; }
    public string Word { get; set; } = null!;
    public string English { get; set; } = null!;
    public string? Indonesia { get; set; } 
    public string? Korea { get; set; } 
    public string? Arabic { get; set; } 
    public string? Chinese { get; set; } 
}
