
using Saga.DomainShared.Enums;
using Saga.DomainShared.Models;

namespace Saga.Infrastructure.Interfaces;

public interface IAuditLogger
{
    Task WriteLoggerAsync(LogMode mode, ContentOf content, object obj);
    Task<IEnumerable<UserLogger>> ReadLoggerAsync(DateOnly startDate, DateOnly endDate);
}
