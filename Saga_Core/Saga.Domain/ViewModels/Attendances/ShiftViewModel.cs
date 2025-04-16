using Microsoft.AspNetCore.Mvc.Rendering;
using Saga.Domain.Dtos.Attendances;

namespace Saga.Domain.ViewModels.Attendances;

public class ShiftItemList 
{
    public Guid Key { get; set; }
    public Guid? CompanyKey { get; set; } = Guid.Empty;
    public string? ShiftGroupName { get; set; } = String.Empty;
    public Company? Company { get; set; }
}

public class ShiftList
{
    public IEnumerable<ShiftItemList> Shifts { get; set; } = new List<ShiftItemList>();
}

public class ShiftForm : ShiftItemList
{
    public List<SelectListItem> GroupExisting { get; set; } = [];
    public List<SelectListItem> Companies { get; set; } = [];
    public List<SelectListItem> Days { get; set; } = [];
    public List<SelectListItem> WorkTypes { get; set; } = [];

    public IEnumerable<ShiftDetailForm> ShiftDetails { get; set; }

    //For Deserialization or Serialization Input form array
    public string JsonShiftDetails { get; set; } = string.Empty;

    public int? MaxLimit { get; set; } = 0;

    public string? Description { get; set; } = String.Empty;

    public Guid CompanySelected { get; set; } = Guid.Empty;
    public Guid GroupSelected { get; set; } = Guid.Empty;

    public ShiftDto ConvertToShiftDto()
    {
        return new ShiftDto
        {
            Key = this.Key,
            CompanyKey = this.CompanyKey ?? Guid.Empty,
            ShiftGroupName = this.ShiftGroupName ?? String.Empty,
            MaxLimit = this.MaxLimit,
            Description = this.Description,
            ShiftDetails = this.ShiftDetails?
                               .Select(sd => sd.ConvertToShiftDetailDto()).ToList() 
                               ?? new List<ShiftDetailDto>()
        };
    }
}
