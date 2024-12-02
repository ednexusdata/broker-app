using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Service.Resolvers;

namespace EdNexusData.Broker.Web.ViewModels;

public class RequestCardViewModel
{
    public Guid Id { get; set; }
    public string ReleasingDistrictState { get; set; } = string.Empty;
    public string ReleasingDistrict { get; set; } = string.Empty;
    public string ReleasingSchool { get; set; } = string.Empty;
    public string ReceivingDistrictState { get; set; } = string.Empty;
    public string ReceivingDistrict { get; set; } = string.Empty;
    public string ReceivingSchool { get; set; } = string.Empty;

    public string ReleasingSchoolDisplay { 
        get {
            return $"{ReleasingDistrictState}/{ReleasingDistrict}/{ReleasingSchool}";
        }
    }
    public string ReceivingSchoolDisplay { 
        get {
            return $"{ReceivingDistrictState}/{ReceivingDistrict}/{ReceivingSchool}";
        }
    }

    public string? Student { get; set; } = string.Empty;
    public string? StudentId { get; set; } = string.Empty;
    public string? RecordType { get; set; } = string.Empty;
    public string? Date { get; set; }
    public string? User { get; set; }

    public RequestCardViewModel(Request request, TimeZoneInfo timeZoneInfo)
    {
        Id = request.Id;
        ReleasingDistrictState = request.RequestManifest?.To?.District?.Address?.StateAbbreviation ?? string.Empty;
        ReleasingDistrict = request.RequestManifest?.To?.District?.ShortName ?? request.RequestManifest?.To?.District?.Name ?? string.Empty;
        ReleasingSchool = request.RequestManifest?.To?.School?.ShortName ?? request.RequestManifest?.To?.School?.Name ?? string.Empty;
        ReceivingDistrictState = request.RequestManifest?.From?.District?.Address?.StateAbbreviation ?? string.Empty;
        ReceivingDistrict = request.RequestManifest?.From?.District?.ShortName ?? request.RequestManifest?.From?.District?.Name ?? string.Empty;
        ReceivingSchool = request.RequestManifest?.From?.School?.ShortName ?? request.RequestManifest?.From?.School?.Name ?? string.Empty;
        Student = $"{request.RequestManifest?.Student?.LastName}, {request.RequestManifest?.Student?.FirstName}";
        StudentId = request.RequestManifest?.Student?.StudentNumber;
        RecordType = TypeResolver.ResolveDisplayName(request.Payload)?.DisplayName ?? request.Payload;
        Date = TimeZoneInfo.ConvertTimeFromUtc(request.CreatedAt.DateTime, timeZoneInfo).ToString("M/dd/yyyy h:mm tt");
        User = request.RequestProcessUser?.Name ?? request.ResponseProcessUser?.Name;
    }
}