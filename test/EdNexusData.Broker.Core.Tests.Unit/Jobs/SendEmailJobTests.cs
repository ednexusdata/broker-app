using System.Text.Json;
using EdNexusData.Broker.Core.Emails.ViewModels;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Worker;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.MailKitSmtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using Moq;

namespace EdNexusData.Broker.Core.Jobs.Tests.Unit;

/// <summary>
/// Sends through the real MailKit sender, which writes to a pickup directory instead of an SMTP server,
/// so the tests see the message exactly as it would be sent.
/// </summary>
public class SendEmailJobTests : IDisposable
{
    private readonly string pickupDirectory = Path.Combine(Path.GetTempPath(), "broker-email-" + Guid.NewGuid().ToString("N"));
    private readonly SendEmailJob job;

    public SendEmailJobTests()
    {
        Directory.CreateDirectory(pickupDirectory);

        var sender = new MailKitSender(new SmtpClientOptions { UsePickupDirectory = true, MailPickupDirectory = pickupDirectory });
        var renderer = new FixedTemplateRenderer();
        var fluentEmail = new Email(renderer, sender, "broker@example.test");

        var jobStatusService = new JobStatusService<SendEmailJob>(
            new Mock<ILogger<SendEmailJob>>().Object,
            new Mock<IRepository<Job>>().Object,
            new Mock<IRepository<Request>>().Object,
            new Mock<IRepository<Message>>().Object,
            new Mock<IRepository<PayloadContentAction>>().Object,
            new JobStatusStore());

        var services = new ServiceCollection()
            .AddSingleton(new SettingsService(new Mock<IRepository<Setting>>().Object, new Mock<IReadRepository<Setting>>().Object))
            .BuildServiceProvider();

        job = new SendEmailJob(jobStatusService, fluentEmail, renderer, new TestEnvironment(services.GetRequiredService<IServiceScopeFactory>()));
    }

    public void Dispose() => Directory.Delete(pickupDirectory, recursive: true);

    [Fact]
    public async Task Sends_without_a_reply_to_address()
    {
        await job.ProcessAsync(EmailJob(to: "district@example.test", replyTo: null));

        var message = SentMessage();
        Assert.Equal("district@example.test", Assert.Single(message.To.Mailboxes).Address);
        Assert.Empty(message.ReplyTo);
    }

    [Fact]
    public async Task Sends_with_a_reply_to_address()
    {
        await job.ProcessAsync(EmailJob(to: "district@example.test", replyTo: "sender@example.test"));

        Assert.Equal("sender@example.test", Assert.Single(SentMessage().ReplyTo.Mailboxes).Address);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Fails_with_a_clear_message_when_there_is_no_recipient(string? to)
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => job.ProcessAsync(EmailJob(to, replyTo: "sender@example.test")));

        Assert.Contains("no recipient", ex.Message);
        Assert.Empty(Directory.GetFiles(pickupDirectory));
    }

    private static Job EmailJob(string? to, string? replyTo) => new()
    {
        Id = Guid.NewGuid(),
        JobParameters = JsonSerializer.SerializeToDocument(new EmailJobDetail
        {
            TemplateName = "RetentionReminder",
            To = to,
            ReplyTo = replyTo,
            Subject = "Test email",
            Model = new RetentionReminderViewModel { RequestId = Guid.NewGuid(), DaysRemaining = 3 },
            ModelType = typeof(RetentionReminderViewModel).FullName
        })
    };

    private MimeMessage SentMessage() => MimeMessage.Load(Assert.Single(Directory.GetFiles(pickupDirectory)));

    private class FixedTemplateRenderer : ITemplateRenderer
    {
        public string Parse<T>(string template, T model, bool isHtml = true) => "<p>Test</p>";

        public Task<string> ParseAsync<T>(string template, T model, bool isHtml = true) => Task.FromResult(Parse(template, model, isHtml));
    }

    private class TestEnvironment(IServiceScopeFactory serviceScopeFactory) : Environment(serviceScopeFactory);
}
