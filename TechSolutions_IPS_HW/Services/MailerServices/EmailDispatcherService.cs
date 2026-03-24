using TechSolutions_IPS_HW.Models.Enums;
using TechSolutions_IPS_HW.Repositories.Interfaces;

namespace TechSolutions_IPS_HW.Services.MailerServices
{
    /// <summary>
    /// Background service that polls the EmailQueueEntries table every 5 minutes,
    /// removes duplicate unsent emails Failed sends are retried up to 3 times 
    /// with back-off policy.
    /// </summary>
    public class EmailDispatcherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailDispatcherService> _logger;

        private const int MaxRetries = 3;
        private const int BatchSize = 20;
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

        public EmailDispatcherService(
            IServiceScopeFactory scopeFactory, 
            ILogger<EmailDispatcherService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        //Starts execution of the background service, which runs until the application is stopped.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RemoveDuplicatesAsync(stoppingToken);
                    await ProcessQueueAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email queue");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Finds duplicate pending emails same ToAddress, Subject and Body and removes
        /// all but the most recent entry for each group.     
        /// </summary>
        private async Task RemoveDuplicatesAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEmailQueueRepository>();

            var deleted = await repo.RemoveDuplicatesAsync(stoppingToken);

            if (deleted > 0)
            {
                _logger.LogInformation("Removed {Count} duplicate pending email(s) from queue", deleted);
            }
        }

        //Checks the db table for pending emails that havvcent been sent
        //this gets called every 5 minutes and then sends unsent mails
        //TODO : Check email templates as well as part of the duplicate removal and sending process
        private async Task ProcessQueueAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IEmailQueueRepository>();
            var sender = scope.ServiceProvider.GetRequiredService<Interfaces.IEmailSender>();

            var now = DateTime.UtcNow;

            var pending = await repo.GetPendingBatchAsync(BatchSize, now, stoppingToken);

            foreach (var entry in pending)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await sender.SendEmailAsync(entry.ToAddress, entry.Subject, entry.Body);

                    entry.Status = EmailQueueStatus.Sent;
                    entry.SentAt = DateTime.UtcNow;
                    entry.LastError = null;

                    _logger.LogInformation("Email sent to {To} (QueueId: {Id})", entry.ToAddress, entry.Id);
                }
                catch (Exception ex)
                {
                    entry.RetryCount++;
                    entry.LastError = ex.Message;

                    if (entry.RetryCount >= MaxRetries)
                    {
                        entry.Status = EmailQueueStatus.Failed;
                        _logger.LogError(ex, "Email to {To} permanently failed after {Retries} retries (QueueId: {Id})", entry.ToAddress, entry.RetryCount, entry.Id);
                    }
                    else
                    {
                        // Exponential back-off: 30s, 60s, 120s, 240s ...
                        entry.NextRetryAt = DateTime.UtcNow.AddSeconds(30 * Math.Pow(2, entry.RetryCount - 1));
                        _logger.LogWarning(ex, "Email to {To} failed (attempt {Retry}/{Max}), next retry at {NextRetry} (QueueId: {Id})",
                            entry.ToAddress, entry.RetryCount, MaxRetries, entry.NextRetryAt, entry.Id);
                    }
                }

                await repo.UpdateAsync(entry, stoppingToken);
            }
        }
    }
}
