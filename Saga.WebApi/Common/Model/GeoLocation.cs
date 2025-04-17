namespace SidoAgung.Common.Models;

public class GeoLocation
{
    public string Longitude { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;

    public GeoLocation() { }
    public GeoLocation(string longitude, string latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }
}
