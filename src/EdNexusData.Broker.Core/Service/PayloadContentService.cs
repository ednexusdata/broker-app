using System.Text.Json;
using EdNexusData.Broker.Core.Specifications;

namespace EdNexusData.Broker.Core.Services;

public class PayloadContentService
{
    private readonly IRepository<PayloadContent> payloadContentRepository;

    public PayloadContentService(IRepository<PayloadContent> payloadContentRepository)
    {
        this.payloadContentRepository = payloadContentRepository;
    }

    public async Task<MultipartFormDataContent> AddAttachments(MultipartFormDataContent multipartFormDataContent, Message message)
    {
        var attachments = await payloadContentRepository.ListAsync(new PayloadContentsByMessageId(message.Id));
        if (attachments is not null && attachments.Count > 0)
        {
            foreach(var attachment in attachments)
            {
                if (attachment.BlobContent != null)
                {
                    multipartFormDataContent.Add(new ByteArrayContent(attachment.BlobContent!), "files", attachment.FileName!);
                }
                if (attachment.JsonContent != null)
                {
                    multipartFormDataContent.Add(new StringContent(JsonSerializer.Serialize(attachment.JsonContent)), "files", attachment.FileName!);
                }
            }
        }
        return multipartFormDataContent;
    }
}