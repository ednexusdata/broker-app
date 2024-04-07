#nullable disable
namespace EdNexusData.Broker.Web.Models.JsonDocuments;

public class StudentJsonModel
{
    public string Id { get; set; }
    public string StudentUniqueId { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastSurname { get; set; }
    public string BirthDate { get; set; }
    public string Gender { get; set; }
    public string Grade { get; set; }
}
