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

    public async Task<List<PayloadContent?>> ProcessFiles(Message message, List<Models.File> files)
    {
        var payloadContents = new List<PayloadContent?>();

        if (files.Count > 0)
        {
            foreach(var file in files)
            {
                payloadContents.Add(await AddFile(message, file));
            }
        }

        return payloadContents;
    }

    public async Task<PayloadContent?> AddJsonFile(Guid requestId, JsonDocument content, string contentType, string fileName)
    {
        var payloadContent = new PayloadContent()
        {
            RequestId = requestId,
            JsonContent = JsonSerializer.SerializeToDocument(content),
            ContentType = contentType,
            FileName =  fileName
        };
        return await payloadContentRepository.AddAsync(payloadContent);
    }

    public async Task<PayloadContent?> AddBlobFile(Guid requestId, byte[] content, string contentType, string fileName)
    {
        var payloadContent = new PayloadContent()
        {
            RequestId = requestId,
            JsonContent = JsonSerializer.SerializeToDocument(content),
            ContentType = contentType,
            FileName =  fileName
        };
        return await payloadContentRepository.AddAsync(payloadContent);
    }

    public async Task<PayloadContent?> AddFile(Message message, Models.File file)
    {
        ManifestContent? fileContentType;
        Request request = message.Request!;

        if (file.Contents is not null)
        {
            if (request is not null && request.ResponseManifest is not null)
            {
                fileContentType = request!.ResponseManifest?.Contents?.Where(i => i.FileName == file.Name).FirstOrDefault();
            }
            else
            {
                fileContentType = request!.RequestManifest?.Contents?.Where(i => i.FileName == file.Name).FirstOrDefault();
            }

            var messageContent = new PayloadContent()
            {
                Id = Guid.NewGuid(),
                RequestId = request.Id,
                MessageId = message!.Id,
                ContentType = fileContentType?.ContentType,
                FileName = file.Name
            };

            if (messageContent.ContentType == "application/json")
            {
                messageContent.JsonContent = JsonDocument.Parse(System.Text.Encoding.Default.GetString(file.Contents));
            }
            else
            {
                messageContent.BlobContent = file.Contents;
            }

            await payloadContentRepository.AddAsync(messageContent);
            
            return messageContent;
        }

        return null;
    }
}