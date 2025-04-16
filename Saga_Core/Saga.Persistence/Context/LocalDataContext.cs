
using Microsoft.EntityFrameworkCore;
using Saga.DomainShared.Models;

namespace Saga.Persistence.Context;

public class LocalDataContext(DbContextOptions<LocalDataContext> options) : DbContext(options)
{
    public DbSet<UserLogger> UserLogger { get; set; }
    public DbSet<ApplicationLanguage> Language { get; set; }
}
