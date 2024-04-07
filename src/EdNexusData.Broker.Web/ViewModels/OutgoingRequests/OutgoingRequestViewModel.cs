using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Extensions;
using EdNexusData.Broker.Web.Constants.DesignSystems;

namespace EdNexusData.Broker.Web.ViewModels.OutgoingRequests;

public class OutgoingRequestViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Display(Name = "Receiving District")]
    public string ReceivingDistrict { get; set; } = string.Empty;

    [Display(Name = "Receiving School")]
    public string ReceivingSchool { get; set; } = string.Empty;

    [Display(Name = "Releasing District")]
    public string ReleasingDistrict { get; set; } = string.Empty;

    [Display(Name = "Releasing School")]
    public string ReleasingSchool { get; set; } = string.Empty;

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

    public OutgoingRequestViewModel() { }

    public OutgoingRequestViewModel(Request outgoingRequest)
    {
        Id = outgoingRequest.Id;
        ReceivingDistrict = outgoingRequest.RequestManifest?.From?.District?.Name ?? string.Empty;
        ReceivingSchool = outgoingRequest.RequestManifest?.From?.School?.Name ?? string.Empty;
        ReleasingDistrict = outgoingRequest.EducationOrganization?.ParentOrganization?.Name ?? string.Empty;
        ReleasingSchool = outgoingRequest.EducationOrganization?.Name ?? string.Empty;
        Student = $"{outgoingRequest.RequestManifest?.Student?.LastName}, {outgoingRequest.RequestManifest?.Student?.FirstName}";

        var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        Date = TimeZoneInfo.ConvertTimeFromUtc(outgoingRequest.CreatedAt.DateTime, pacific).ToString("M/dd/yyyy h:mm tt");
        
        Status = outgoingRequest.RequestStatus.GetDescription();
    }

    public OutgoingRequestViewModel(
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
