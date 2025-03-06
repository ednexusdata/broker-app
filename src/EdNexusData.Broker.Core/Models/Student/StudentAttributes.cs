namespace EdNexusData.Broker.Core;

public class StudentAttributes
{
    public string? Grade { get; set; }
    public string? Gender { get; set; }
    public District? District { get; set; }
    public School? School { get; set; }

    public Common.Students.StudentAttributes ToCommon()
    {
        return new Common.Students.StudentAttributes()
        {
            Grade = Grade,
            Gender = Gender,
            District = District?.ToCommonEducationOrganization(),
            School = School?.ToCommonEducationOrganization()
        };
    }
}