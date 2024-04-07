using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Models.Results;

namespace EdNexusData.Broker.Web.Services.PayloadContents;

public class PayloadContentService : IPayloadContentService
{
    private readonly IRepository<PayloadContent> _payloadContentRepository;

    public PayloadContentService(IRepository<PayloadContent> payloadContentRepository)
    {
        _payloadContentRepository = payloadContentRepository;
    }

    public async Task<IServiceResult<string>> AddPayloadContentsAsync(
        IFormFileCollection files,
        Guid requestId)
    {
        try
        {
            var payloadContents = await ToPayloadContentsAsync(files, requestId);
            await _payloadContentRepository.AddRangeAsync(payloadContents);
            await _payloadContentRepository.SaveChangesAsync();
            return ServiceResult<string>.CreateSuccess("Payload contents added successfully.");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.CreateFailure("An error occurred while adding payload contents.", ex);
        }
    }


    public async Task<IEnumerable<PayloadContent>> ToPayloadContentsAsync(
        IFormFileCollection files,
        Guid requestId)
    {
        var payloadContentTasks = files.Select(async file =>
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            return new PayloadContent
            {
                // RequestId = requestId,
                // RequestResponse = RequestResponse.Request,
                ContentType = file.ContentType,
                BlobContent = fileBytes,
            };
        });

        return await Task.WhenAll(payloadContentTasks);
    }
}

