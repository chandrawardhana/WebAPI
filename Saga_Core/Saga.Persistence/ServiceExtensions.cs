using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Saga.DomainShared.Interfaces;
using Saga.DomainShared.Services;
using Saga.Persistence.Context;
using Saga.Persistence.Models;

namespace Saga.Persistence;

public static class ServiceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services //.AddEntityFrameworkNpgsql()
                .AddDbContext<DataContext>(opt =>
                        opt.UseNpgsql(
                            configuration.GetConnectionString("DefaultConnection"),
                            b => {
                                b.MigrationsAssembly(typeof(DataContext).Assembly.FullName);
                            }
                        )
                , ServiceLifetime.Transient);

        services.AddTransient<IDataContext>(provider => provider.GetRequiredService<DataContext>());

        services.AddDatabaseDeveloperPageExceptionFilter();
        
        services.AddDefaultIdentity<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<DataContext>()
                .AddSignInManager()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddDefaultTokenProviders();

        services.AddDbContext<LocalDataContext>(opt => 
                opt.UseSqlite(configuration.GetConnectionString("LocalStorage")));

        return services;
    }
}
