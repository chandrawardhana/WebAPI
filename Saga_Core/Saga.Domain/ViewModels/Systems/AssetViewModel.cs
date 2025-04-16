namespace Saga.Domain.ViewModels.Systems;

public class AssetList
{
    public Guid Key { get; set; }
    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? MimeType { get; set; }
    public DateTime? UploadAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class AssetForm
{
    public Guid Key { get; set; }
    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? MimeType { get; set; }
    public DateTime? UploadAt { get; set; }
    public string FilePath { get; set; } = string.Empty;
    //to store file bytes
    public byte[]? FileData { get; set; }
}
