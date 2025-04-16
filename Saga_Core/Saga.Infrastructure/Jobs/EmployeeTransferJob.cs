using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Saga.Mediator.Employees.EmployeeTransferMediator;


namespace Saga.Infrastructure.Jobs
{
    public class EmployeeTransferJob(
                                        ILogger<EmployeeTransferJob> _logger,
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

                    await ProcessPendingTransfers(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing employee transfers");
                    await Task.Delay(interval, stoppingToken);
                }
            }
        }

        private async Task ProcessPendingTransfers(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var _transferLogger = scope.ServiceProvider.GetRequiredService<ILogger<EmployeeTransferJob>>();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var pendingTransfers = await mediator.Send(new GetPendingEmployeeTransfersQuery(today), stoppingToken);

            foreach (var transfer in pendingTransfers)
            {
                try
                {
                    var result = await mediator.Send(new ProcessEmployeeTransferCommand(transfer), stoppingToken);
                    if (result.Succeeded)
                        _transferLogger.LogInformation("Succesfully processed transfer for employee {EmployeeKey}",
                                                transfer.EmployeeKey);
                }
                catch (Exception ex)
                {
                    _transferLogger.LogError(ex,"Error processing transfer for employee {EmployeeKey}",
                                        transfer.EmployeeKey);
                }
            }
        }
    }
}
