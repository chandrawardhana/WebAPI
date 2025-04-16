namespace Saga.Domain.Dtos.Systems;

public class AssetDto
{
    public Guid? Key { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public string MimeType { get; set; }
    public DateTime UploadAt { get; set; }
}
