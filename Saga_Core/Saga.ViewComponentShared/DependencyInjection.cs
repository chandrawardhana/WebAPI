
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Saga.ViewComponentShared.Interfaces;
using Saga.ViewComponentShared.Services;
using Saga.ViewComponentShared.ViewComponents;
using System.Reflection;

namespace Saga.ViewComponentShared;

/// <summary>
/// ashari.herman 2025-03-20 slipi jakarta
/// </summary>

public static class DependencyInjection
{
    public static IServiceCollection AddSagaViewComponent(this IServiceCollection services, IConfiguration configuration)
    {
        List<Assembly> assemblies = [
            typeof(BasicFormFilterViewComponent).Assembly
        ];

        assemblies.ForEach(assembly =>
        {
            services.AddMvcCore().AddApplicationPart(assembly).AddRazorRuntimeCompilation();

        });

        services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
        {
            assemblies.ForEach(assembly =>
            {
                options.FileProviders.Add(new EmbeddedFileProvider(assembly));
            });
        });

        services.AddTransient<IFormFilter, FormFilterService>();

        return services;
    }
}
