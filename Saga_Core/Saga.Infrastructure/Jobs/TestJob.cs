using Microsoft.Extensions.Hosting;

namespace Saga.Infrastructure.Jobs
{
    public class TestJob : BackgroundService
    {
        private readonly TimeSpan interval = TimeSpan.FromSeconds(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new(interval);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                Console.WriteLine(DateTime.Now);
            }
        }
    }
}
