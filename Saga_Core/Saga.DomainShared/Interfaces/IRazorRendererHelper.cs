
namespace Saga.DomainShared.Interfaces;

public interface IRazorRendererHelper
{
    Task<string> RenderViewToString<T>(string partialName, T model);
}
