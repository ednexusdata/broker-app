using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Extensions;
using EdNexusData.Broker.Web.Constants.DesignSystems;

namespace EdNexusData.Broker.Web.ViewModels.IncomingRequests;

public class IncomingRequestViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Display(Name = "Releasing District")]
    public string ReleasingDistrict { get; set; } = string.Empty;

    [Display(Name = "Releasing School")]
    public string ReleasingSchool { get; set; } = string.Empty;

    [Display(Name = "Receiving District")]
    public string ReceivingDistrict { get; set; } = string.Empty;

    [Display(Name = "Receiving School")]
    public string ReceivingSchool { get; set; } = string.Empty;

    [Display(Name = "Student")]
    public string? Student { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date")]
    public string? Date { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    // TODO: Map Status to proper Voice Tone.
    [Display(Name = "Status Tone")]
    public string StatusTone => VoiceTone.Positive;
    public IncomingRequestViewModel() { }

    public IncomingRequestViewModel(Request incomingRequest)
    {

        Id = incomingRequest.Id;
        ReleasingDistrict = incomingRequest.RequestManifest?.To?.District?.Name ?? string.Empty;
        ReleasingSchool = incomingRequest.RequestManifest?.To?.School?.Name ?? string.Empty;
        ReceivingDistrict = incomingRequest.EducationOrganization?.ParentOrganization?.Name ?? string.Empty;
        ReceivingSchool = incomingRequest.EducationOrganization?.Name ?? string.Empty;
        Student = $"{incomingRequest.RequestManifest?.Student?.LastName}, {incomingRequest.RequestManifest?.Student?.FirstName}";
        
        var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        Date = (incomingRequest.InitialRequestSentDate != null) ?
                TimeZoneInfo.ConvertTimeFromUtc(incomingRequest.InitialRequestSentDate.Value.DateTime, pacific).ToString("M/dd/yyyy h:mm tt") 
                : null;

        Status = incomingRequest.RequestStatus.GetDescription();
    }
    public IncomingRequestViewModel(
        Guid id,
        string releasingDistrict,
        string releasingSchool,
        string receivingDistrict,
        string receivingSchool,
        string? student,
        DateTime date,
        string status)
    {
        Id = id;
        ReleasingDistrict = releasingDistrict;
        ReleasingSchool = releasingSchool;
        ReceivingDistrict = receivingDistrict;
        ReceivingSchool = receivingSchool;
        Student = student;
        Date = date.ToString("M/dd/yyyy h:mm tt");
        Status = status;
    }
}
