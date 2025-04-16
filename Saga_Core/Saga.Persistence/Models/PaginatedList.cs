using Microsoft.EntityFrameworkCore;
using Saga.DomainShared;

namespace Saga.Persistence.Models;

public class PaginatedList<T>(List<T> items, int count, int pageNumber, int pageSize) 
    : Pagination<T>(items, count, pageNumber, pageSize)
{
    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
