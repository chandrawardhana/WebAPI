using Saga.DomainShared.Interfaces;

namespace Saga.DomainShared.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
