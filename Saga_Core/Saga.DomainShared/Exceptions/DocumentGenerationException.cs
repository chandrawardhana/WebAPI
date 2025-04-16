
namespace Saga.DomainShared.Exceptions;

public class DocumentGenerationException : Exception
{
    public DocumentGenerationException(string message) : base(message) { }
    public DocumentGenerationException(string message, Exception innerException)
        : base(message, innerException) { }
}
