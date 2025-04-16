using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class FingerPrintListItem
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? Name { get; set; } = String.Empty;
    public string? IPAddress { get; set; } = String.Empty;
    public ConnectionMethod Method { get; set; }
    public TimeSpan[]? RetrieveScheduleTimes { get; set; }
    public string? SerialNumber { get; set; } = String.Empty;
    public Company? Company { get; set; }
}

public class FingerPrintList
{
    public IEnumerable<FingerPrintListItem> FingerPrintLists { get; set; } = new List<FingerPrintListItem>();
}

public class FingerPrintForm : FingerPrintListItem
{
    public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
    public int? Port { get; set; } = 0;
    public string? CommKey { get; set; } = String.Empty;
    public string? Comm { get; set; } = String.Empty;
    public int? Baudrate { get; set; } = 0;
    public string? Description { get; set; } = String.Empty;
    
    //For Deserialization or Serialization Input form array
    public string JsonRetrieveScheduleTimes { get; set; } = String.Empty;

    public FingerPrintDto ConvertToFingerPrintDto()
    {
        TimeSpan[]? retrieveScheduleTimes = null;

        if (!string.IsNullOrEmpty(JsonRetrieveScheduleTimes))
        {
            var scheduleObjects = JsonConvert.DeserializeObject<dynamic[]>(JsonRetrieveScheduleTimes);
            if (scheduleObjects != null)
            {
                // Extract only the RetrieveScheduleTimes values and convert to TimeSpan
                retrieveScheduleTimes = scheduleObjects
                    .Select(item => TimeSpan.TryParse(item.RetrieveScheduleTimes.ToString(), out TimeSpan time) ? time : TimeSpan.Zero)
                    .Where(time => time != TimeSpan.Zero)
                    .ToArray();
            }
        }

        return new FingerPrintDto
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey,
            Code = this.Code,
            Name = this.Name,
            IPAddress = this.IPAddress,
            Method = this.Method,
            Port = this.Port,
            CommKey = this.CommKey,
            Comm = this.Comm,
            Baudrate = this.Baudrate,
            Description = this.Description,
            RetrieveScheduleTimes = retrieveScheduleTimes ?? Array.Empty<TimeSpan>()
        };
    }

    public TestConnectionDto ConvertToTestConnectionDto()
    {
        return new TestConnectionDto
        {
            IPAddress = this.IPAddress,
            Method = this.Method,
            Port = this.Port,
            Comm = this.Comm,
            Baudrate = this.Baudrate,
            SerialNumber = this.SerialNumber,
        };
    }
}

public class RetrieveDataFingerPrint
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? Code { get; set; } = String.Empty;
    public string? Name { get; set; } = String.Empty;
    public string? IPAddress { get; set; } = String.Empty;
    public int? Port { get; set; }
    public string? CommKey { get; set; } = String.Empty;
    public bool Status { get; set; }
    public Company? Company { get; set; }
}
