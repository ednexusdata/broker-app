namespace EdNexusData.Broker.Web.ViewModels.Preparing;

public class CreateRequestManifestViewModel
{
    public Guid RequestId { get; set; }

    public List<CreateRequestManifestItemViewModel>? Items { get; set; }
}

public class CreateRequestManifestItemViewModel
{
    public Guid? PayloadContentId { get; set;}
    public string? PayloadContentAction { get; set; }
}