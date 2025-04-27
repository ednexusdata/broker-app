using System.ComponentModel;
using System.Text.Json;
using EdNexusData.Broker.Core.Emails;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Worker;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Send Email")]
public class SendEmailJob : IJob
{
    private IFluentEmail fluentEmail;
    private readonly ITemplateRenderer templateRenderer;

    public SendEmailJob(
        IFluentEmail fluentEmail,
        ITemplateRenderer templateRenderer
    
    )
    {
        this.fluentEmail = fluentEmail;
        this.templateRenderer = templateRenderer;
    }
    
    public async Task ProcessAsync(Job jobRecord)
    {
        // Get job detail
        _ = jobRecord.JobParameters ?? throw new ArgumentNullException("Missing job parameters");
        var jobDetail = JsonSerializer.Deserialize<EmailJobDetail>(jobRecord.JobParameters);
        _ = jobDetail ?? throw new ArgumentNullException("Unable to deserialize job parameters to EmailJobDetail");

        var model = JsonSerializer.Deserialize(jobDetail.Model!.ToString()!, Type.GetType(jobDetail.ModelType!)!);
        
        await fluentEmail
            .To(jobDetail.To)
            .ReplyTo(jobDetail.ReplyTo)
            .Subject(jobDetail.Subject)
            .UsingTemplateFromEmbedded($"EdNexusData.Broker.Core.Emails.{jobDetail.TemplateName}.cshtml", model, typeof(EmailRoot).Assembly)
            .SendAsync();
    }
}