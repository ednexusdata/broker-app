using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Web.Models.Results;

namespace EdNexusData.Broker.Web.Services.PayloadContents
{
    public interface IPayloadContentService
    {
        Task<IServiceResult<string>> AddPayloadContentsAsync(
            IFormFileCollection files,
            Guid requestId);

        Task<IEnumerable<PayloadContent>> ToPayloadContentsAsync(
            IFormFileCollection files,
            Guid requestId);
    }
}