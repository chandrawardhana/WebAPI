using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Saga.Mediator.Services;
using Saga.Validators;
using System.Reflection;

namespace Saga.Mediator;

public static class ServiceExtension
{
    public static void AddMediator(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidator();
        services.AddTransient<IAttendanceRepository, AttendanceRepository>();
        services.AddTransient<IEmployeeRepository, EmployeeRepository>();
        services.AddTransient<IOrganizationRepository, OrganizationRepository>();
        services.AddTransient<IApprovalTransactionRepository, ApprovalTransactionRepository>();
        services.AddHttpClient();
    }
}
