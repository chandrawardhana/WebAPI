using Saga.Domain.Entities.Attendance;

namespace Saga.Domain.Dtos.Attendances;

public class FingerPrintDto
{
    public Guid? Key { get; set; } = Guid.Empty;
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? Name { get; set; } = String.Empty;
    public string? IPAddress { get; set; } = String.Empty;
    public ConnectionMethod Method { get; set; }
    public int? Port { get; set; } = 0;
    public string? CommKey { get; set; } = String.Empty;
    public string? Comm { get; set; } = String.Empty;
    public int? Baudrate { get; set; } = 0;
    public string? Description { get; set; } = String.Empty;
    public TimeSpan[]? RetrieveScheduleTimes { get; set; }
    public string? SerialNumber { get; set; } = String.Empty;

    public FingerPrint ConvertToEntity()
    {
        return new FingerPrint
        {
            Key = this.Key ?? Guid.Empty,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            Code = this.Code ?? String.Empty,
            Name = this.Name ?? String.Empty,
            IPAddress = this.IPAddress ?? String.Empty,
            Method = this.Method,
            Port = this.Port ?? 0,
            CommKey = this.CommKey ?? String.Empty,
            Comm = this.Comm ?? String.Empty,
            Baudrate = this.Baudrate ?? 0,
            Description = this.Description ?? String.Empty,
            RetrieveScheduleTimes = this.RetrieveScheduleTimes ?? Array.Empty<TimeSpan>(),
            SerialNumber = this.SerialNumber ?? String.Empty
        };
    }
}

public class TestConnectionDto
{
    public string? IPAddress { get; set; } = String.Empty;
    public ConnectionMethod Method { get; set; }
    public int? Port { get; set; } = 0;
    public string? Comm { get; set; } = String.Empty;
    public int? Baudrate { get; set; } = 0;
    public string? SerialNumber { get; set; } = String.Empty;
}
