using System.ComponentModel.DataAnnotations;

namespace SidoAgung.Saga.Common.Dtos;

public class LoginDto
{
    [Required]
    public string Username {get; set;} = null!;
    [Required]
    public string Password {get; set;} = null!;
}

public class ChangePassword
{
    [Required]
    public string F1 { get; set; } = null!;
    [Required]
    public string F2 { get; set; } = null!;
}
