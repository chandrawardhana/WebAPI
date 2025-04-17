namespace SidoAgung.Saga.Common.Model;

public class Attachment
{
    public Guid AssetKey { get; set; } = Guid.Empty;
    public string Base64Data { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public string ToSource()
        => string.Format(
            "data:{0};base64,{1}",
            //"data:{0};{1}base64,{2}", 
            ContentType,
            //string.IsNullOrEmpty(FileName) ? string.Empty : $"headers=filename%3D{FileName};", 
            Base64Data
        );
}
