using System.ComponentModel;
using System.Text.Json;
using EdNexusData.Broker.Core.Emails;
using EdNexusData.Broker.Core.Emails.ViewModels;
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
    private readonly Environment environment;

  public SendEmailJob(
        IFluentEmail fluentEmail,
        ITemplateRenderer templateRenderer,
        Environment environment
    )
    {
        this.fluentEmail = fluentEmail;
        this.templateRenderer = templateRenderer;
        this.environment = environment;
  }
    
    public async Task ProcessAsync(Job jobRecord)
    {
        // Get job detail
        _ = jobRecord.JobParameters ?? throw new ArgumentNullException("Missing job parameters");
        var jobDetail = JsonSerializer.Deserialize<EmailJobDetail>(jobRecord.JobParameters);
        _ = jobDetail ?? throw new ArgumentNullException("Unable to deserialize job parameters to EmailJobDetail");

        var model = JsonSerializer.Deserialize(jobDetail.Model!.ToString()!, Type.GetType(jobDetail.ModelType!)!);

        var baseViewModel = model as BaseViewModel;
        _ = baseViewModel ?? throw new ArgumentNullException("Unable to cast model to BaseViewModel");
        
        baseViewModel.EnvironmentService = environment;

        var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Final-Education-Nexus-ICON-png.png");

        await fluentEmail
            .To(jobDetail.To)
            .ReplyTo(jobDetail.ReplyTo)
            .Subject(jobDetail.Subject)
            .Attach(new FluentEmail.Core.Models.Attachment() { Data = System.IO.File.OpenRead(logoPath), Filename = "brokerlogo.png", ContentId = "brokerlogo", ContentType = "image/png", IsInline = true })
            .UsingTemplateFromEmbedded($"EdNexusData.Broker.Core.Emails.{jobDetail.TemplateName}.cshtml", model, typeof(EmailRoot).Assembly)
            .SendAsync();
    }
}