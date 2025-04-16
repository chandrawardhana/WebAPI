using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Saga.DomainShared.Interfaces;
using Saga.Persistence.Context;
using System.Linq.Expressions;

namespace Saga.Infrastructure.Jobs
{
    public class CalculationAttendanceJob(
                                            ILogger<CalculationAttendanceJob> _logger,
                                            IServiceScopeFactory _scopeFactory,
                                            IConfiguration _configuration
                                         ) : BackgroundService
    {
        //Get config from appsettings JobSettings
        private readonly TimeSpan interval = TimeSpan.FromHours(Convert.ToDouble(_configuration.GetValue<string>("JobSettings:IntervalHour")));
        private readonly TimeSpan checkTime = TimeSpan.FromHours(Convert.ToDouble(_configuration.GetValue<string>("JobSettings:ExecutionHour"))).Add(TimeSpan.FromMinutes(Convert.ToDouble(_configuration.GetValue<string>("JobSettings:ExecutionMinute"))));
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = DateTime.Today.Add(checkTime);

                    if (now > nextRun)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    var delay = nextRun - now;
                    await Task.Delay(delay, stoppingToken);

                    await ProcessCompanyAttendance(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing attendance calculations");
                    await Task.Delay(interval, stoppingToken);
                }
            }
        }
        private async Task ProcessCompanyAttendance(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
            var attendanceService = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
            var calculationLogger = scope.ServiceProvider.GetRequiredService<ILogger<CalculationAttendanceJob>>();

            //var customDate = DateTime.ParseExact("2025-03-12 07:00:00", "yyyy-MM-dd HH:mm:ss", new System.Globalization.CultureInfo("id-ID"));
            //var today = DateOnly.FromDateTime(customDate);
            var today = DateOnly.FromDateTime(DateTime.Now);
            var startDate = today.AddDays(-3);
            var endDate = today;

            try
            {
                var result = await attendanceService.CalculationAttendanceAsync(
                    [],
                    (StartDate: startDate,
                    EndDate: endDate),
                    stoppingToken);

                if (result.Succeeded)
                {
                    calculationLogger.LogInformation(
                        "Successfully processed attendance calculation)");
                }
                else
                {
                    calculationLogger.LogWarning(
                        "Failed to process attendance calculation. Errors: {Errors}", string.Join(", ", result.Errors));
                }
            }
            catch (Exception ex)
            {
                calculationLogger.LogError(ex,
                    "Error processing attendance calculation Errors: {Errors}", ex.Message);
            }
        }
    }
}
