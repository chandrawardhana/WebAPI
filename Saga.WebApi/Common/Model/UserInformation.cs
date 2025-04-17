namespace SidoAgung.Common.Models;

public class UserInformation
{ 
    public Guid EmployeeKey { get; set; } = Guid.Empty;
    public string Token { get; set; } = null!; 
}
