namespace EdNexusData.Broker.Core;

public class MappingStudentAttributes
{
    public StudentAttributes? IncomingStudentAttributes { get; set; }
    public StudentAttributes? OutgoingStudentAttributes { get; set; }

    public Common.Mappings.MappingStudentAttributes ToCommon()
    {
        return new Common.Mappings.MappingStudentAttributes()
        {
            IncomingStudentAttributes = IncomingStudentAttributes?.ToCommon(),
            OutgoingStudentAttributes = OutgoingStudentAttributes?.ToCommon()
        };
    }
}