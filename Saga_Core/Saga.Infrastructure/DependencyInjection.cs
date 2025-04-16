using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Saga.DomainShared.Interfaces;
using Saga.DomainShared.Services;
using Saga.Infrastructure.Services;
using Saga.Persistence;
using Saga.Mediator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DinkToPdf.Contracts;
using DinkToPdf;
using Saga.Infrastructure.Constants;
using Saga.Infrastructure.Jobs;
using Microsoft.Extensions.Hosting;
using Saga.Infrastructure.Interfaces;
using ConnectingApps.SmartInject;
namespace Saga.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddTransient<ICurrentUser, CurrentUserService>();
        services.AddLazyTransient<IUserService, UserService>();

        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        services.AddScoped<IRazorRendererHelper, RazorRendererHelper>();
        services.AddScoped<IDocumentGenerator, DocumentGenerator>();
        services.AddTransient<IAttendanceLogMachine, AttendanceLogMachineService>();
        services.AddTransient<IAttendanceService, AttendanceService>();
        services.AddTransient<IApplicationService, ApplicationService>();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        services.AddPersistence(config);
        services.AddMediator();

        services.Configure<IdentityOptions>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequireUppercase = false;
            opt.Password.RequireLowercase = false;
            opt.Password.RequireNonAlphanumeric = false;
            //opt.Password.RequiredLength = 6;
            opt.Password.RequiredUniqueChars = 0;

            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            opt.Lockout.MaxFailedAccessAttempts = 5;
            opt.Lockout.AllowedForNewUsers = true;

            opt.SignIn.RequireConfirmedEmail = false;
            opt.SignIn.RequireConfirmedPhoneNumber = false;
        });

        services.AddAuthentication(opt =>
            {
                opt.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(opt =>
            {
                opt.LoginPath = "/Application/Expired";
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);

                opt.Cookie.Name = "SAGAERP";
                opt.Cookie.IsEssential = true;
                opt.Cookie.SameSite = SameSiteMode.Strict;
                opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

        services.AddAuthorization();

        services.Configure<CookiePolicyOptions>(opt =>
        {
            opt.CheckConsentNeeded = context => true; // consent required
            opt.MinimumSameSitePolicy = SameSiteMode.Strict;
        });

        services.AddSession(opt =>
        {
            opt.Cookie.Name = "SAGAERP";
            opt.Cookie.IsEssential = true;
            opt.Cookie.SameSite = SameSiteMode.Strict;
            opt.Cookie.SecurePolicy = CookieSecurePolicy.None;
            opt.IdleTimeout = TimeSpan.FromMinutes(60);
            //opt.Cookie.HttpOnly = true;
        });

        services.AddAntiforgery(opt =>
        {
            opt.Cookie.Name = "X-CSRF-TOKEN-SAGA";
            opt.FormFieldName = "X-CSRF-TOKEN-SAGA-FORM";
            opt.HeaderName = "X-CSRF-TOKEN-SAGA-HEADER";
        });

        services.AddHttpClient("FingerprintDevice", client => { })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
        });

        return services;
    }

    public static IServiceCollection AddDefineResourceDirectory(this IServiceCollection services)
    {
        if (!Directory.Exists(PathDirectory.Resources))
            Directory.CreateDirectory(PathDirectory.Resources);

        if (!Directory.Exists(PathDirectory.TempReports))
            Directory.CreateDirectory(PathDirectory.TempReports);

        if (!Directory.Exists(PathDirectory.TempCVReports))
            Directory.CreateDirectory(PathDirectory.TempCVReports);

        return services;
    }

    public static IServiceCollection AddServiceJob(this IServiceCollection services)
    {
        //services.AddHostedService<TestJob>();
        services.AddHostedService<CalculationAttendanceJob>();
        services.AddHostedService<EmployeeTransferJob>();
        services.AddHostedService<RetrieveAttendanceJob>();
        services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore; 
        });

        return services;
    }

    public static IServiceCollection AddAuditTrail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLazyTransient<IAuditLogger, AuditLoggerService>();

        return services;
    }

}
