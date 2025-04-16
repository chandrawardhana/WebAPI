
using Saga.DomainShared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Saga.DomainShared.Models;

public class UserLogger
{
    [Key]
    public Guid LogId { get; set; }
    public Guid CompanyKey { get; set; }
    public DateTime Logdate { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public LogMode LogMode { get; set; }
    public string Code { get; set; } = string.Empty;
}
