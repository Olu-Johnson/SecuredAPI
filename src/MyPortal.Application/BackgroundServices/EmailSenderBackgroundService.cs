using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyPortal.Application.Constants;
using MyPortal.Application.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Interfaces;

namespace MyPortal.Application.BackgroundServices;

public class EmailSenderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EmailSenderBackgroundService> _logger;
    private readonly Dictionary<int, DateTime> _networkLastRunTimes = new();

    public EmailSenderBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<EmailSenderBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Sender Background Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEmailBatchesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing email batches");
            }

            // Check every minute for networks that need to send emails
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("Email Sender Background Service is stopping");
    }

    private async Task ProcessEmailBatchesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Get all active SMTP setups with sending enabled
        var smtpSetups = await unitOfWork.Repository<SmtpSetup>()
            .FindAsync(s => s.IsSendingEnabled);

        foreach (var smtpSetup in smtpSetups)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            if (!smtpSetup.NetworkId.HasValue)
                continue;

            var networkId = smtpSetup.NetworkId.Value;

            // Check if enough time has passed since last run for this network
            if (_networkLastRunTimes.TryGetValue(networkId, out var lastRunTime))
            {
                var minutesSinceLastRun = (DateTime.UtcNow - lastRunTime).TotalMinutes;
                if (minutesSinceLastRun < smtpSetup.BatchIntervalMinutes)
                {
                    continue; // Not time yet for this network
                }
            }

            try
            {
                await ProcessNetworkEmailBatchAsync(networkId, smtpSetup, emailService, unitOfWork, stoppingToken);
                _networkLastRunTimes[networkId] = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email batch for network {NetworkId}", networkId);
            }
        }
    }

    private async Task ProcessNetworkEmailBatchAsync(
        int networkId,
        SmtpSetup smtpSetup,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        CancellationToken stoppingToken)
    {
        // Get approved emails for this network (StatusId = 7 - Approved)
        var emails = await unitOfWork.Repository<Email>()
            .FindAsync(e => e.NetworkId == networkId && e.StatusId == StatusConstants.Approved);

        var emailBatch = emails
            .OrderBy(e => e.CreatedAt)
            .Take(smtpSetup.BatchSize)
            .ToList();

        if (!emailBatch.Any())
        {
            return; // No emails to send for this network
        }

        _logger.LogInformation(
            "Processing {EmailCount} emails for network {NetworkId}",
            emailBatch.Count,
            networkId);

        foreach (var email in emailBatch)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                await SendEmailAsync(email, smtpSetup, emailService, unitOfWork);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email {EmailId}", email.Id);
                await HandleEmailFailureAsync(email, smtpSetup, unitOfWork);
            }
        }
    }

    private async Task SendEmailAsync(
        Email email,
        SmtpSetup smtpSetup,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        // Update last attempt date
        email.LastAttemptDate = DateTime.UtcNow;
        email.RetryCount = (email.RetryCount ?? 0) + 1;

        try
        {
            // Call the actual SMTP sending method
            await emailService.SendSmtpEmailAsync(
                smtpSetup,
                email.To ?? string.Empty,
                email.Subject ?? string.Empty,
                email.Message ?? string.Empty,
                email.From);

            // Mark as successfully sent
            email.StatusId = StatusConstants.EmailSent;
            email.SentDate = DateTime.UtcNow;

            await unitOfWork.Repository<Email>().UpdateAsync(email);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully sent email {EmailId}", email.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email {EmailId}", email.Id);
            throw; // Re-throw to be handled by HandleEmailFailureAsync
        }
    }

    private async Task HandleEmailFailureAsync(
        Email email,
        SmtpSetup smtpSetup,
        IUnitOfWork unitOfWork)
    {
        // Update retry count
        email.RetryCount = (email.RetryCount ?? 0) + 1;
        email.LastAttemptDate = DateTime.UtcNow;

        // Mark as failed if max retries exceeded
        if (email.RetryCount > smtpSetup.MaxRetryAttempts)
        {
            email.StatusId = StatusConstants.EmailFailed;
            _logger.LogWarning(
                "Email {EmailId} marked as failed after {RetryCount} attempts",
                email.Id,
                email.RetryCount);
        }

        await unitOfWork.Repository<Email>().UpdateAsync(email);
        await unitOfWork.SaveChangesAsync();
    }
}
