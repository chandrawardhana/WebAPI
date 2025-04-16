using Saga.Domain.Entities.Employees;
using System.Linq.Expressions;

namespace Saga.DomainShared.Interfaces;

public interface IAttendanceService
{
    Task<Result> CalculationAttendanceAsync(Expression<Func<Employee, bool>>[] wheres, (DateOnly StartDate, DateOnly EndDate) dateRange, CancellationToken cancellationToken = default);
}
