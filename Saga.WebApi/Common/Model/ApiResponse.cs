using System.Net;

namespace SidoAgung.WebApi.Common.Model;

public class ApiResponse<T> where T : class
{
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    //public T ParsingData<T>() => JsonConvert.DeserializeObject<T>(this.Data.ToString());
}

public class ApiResponseError
{
    public string[] Errors { get; set; } = Array.Empty<string>();
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public HttpStatusCode Status { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
