
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Web.Extensions.States;

public static class StateFriendlyString
{
    public const string AL = "AL";
    public const string AK = "AK";
    public const string AZ = "AZ";
    public const string AR = "AR";
    public const string CA = "CA";
    public const string CO = "CO";
    public const string CT = "CT";
    public const string DE = "DE";
    public const string FL = "FL";
    public const string GA = "GA";
    public const string HI = "HI";
    public const string ID = "ID";
    public const string IL = "IL";
    public const string IN = "IN";
    public const string IA = "IA";
    public const string KS = "KS";
    public const string KY = "KY";
    public const string LA = "LA";
    public const string ME = "ME";
    public const string MD = "MD";
    public const string MA = "MA";
    public const string MI = "MI";
    public const string MN = "MN";
    public const string MS = "MS";
    public const string MO = "MO";
    public const string MT = "MT";
    public const string NE = "NE";
    public const string NV = "NV";
    public const string NH = "NH";
    public const string NJ = "NJ";
    public const string NM = "NM";
    public const string NY = "NY";
    public const string NC = "NC";
    public const string ND = "ND";
    public const string OH = "OH";
    public const string OK = "OK";
    public const string OR = "OR";
    public const string PA = "PA";
    public const string RI = "RI";
    public const string SC = "SC";
    public const string SD = "SD";
    public const string TN = "TN";
    public const string TX = "TX";
    public const string UT = "UT";
    public const string VT = "VT";
    public const string VA = "VA";
    public const string WA = "WA";
    public const string WV = "WV";
    public const string WI = "WI";
    public const string WY = "WY";
}

public static class States
{
    private static readonly List<SelectListItem> stateSelectList;

    static States() => stateSelectList = GenerateStateSelectList();

    private static List<SelectListItem> GenerateStateSelectList()
    {
        var selectList = new List<SelectListItem>();

        foreach (State state in Enum.GetValues(typeof(State)))
        {
            var friendlyState = state.ToFriendlyString();
            selectList.Add(new SelectListItem
            {
                Text = friendlyState,
                Value = friendlyState
            });
        }

        return selectList;
    }

    public static List<SelectListItem> GetSelectList() => stateSelectList;

    public static string ToFriendlyString(this State state)
    {
        return state switch
        {
            State.AL => StateFriendlyString.AL,
            State.AK => StateFriendlyString.AK,
            State.AZ => StateFriendlyString.AZ,
            State.AR => StateFriendlyString.AR,
            State.CA => StateFriendlyString.CA,
            State.CO => StateFriendlyString.CO,
            State.CT => StateFriendlyString.CT,
            State.DE => StateFriendlyString.DE,
            State.FL => StateFriendlyString.FL,
            State.GA => StateFriendlyString.GA,
            State.HI => StateFriendlyString.HI,
            State.ID => StateFriendlyString.ID,
            State.IL => StateFriendlyString.IL,
            State.IN => StateFriendlyString.IN,
            State.IA => StateFriendlyString.IA,
            State.KS => StateFriendlyString.KS,
            State.KY => StateFriendlyString.KY,
            State.LA => StateFriendlyString.LA,
            State.ME => StateFriendlyString.ME,
            State.MD => StateFriendlyString.MD,
            State.MA => StateFriendlyString.MA,
            State.MI => StateFriendlyString.MI,
            State.MN => StateFriendlyString.MN,
            State.MS => StateFriendlyString.MS,
            State.MO => StateFriendlyString.MO,
            State.MT => StateFriendlyString.MT,
            State.NE => StateFriendlyString.NE,
            State.NV => StateFriendlyString.NV,
            State.NH => StateFriendlyString.NH,
            State.NJ => StateFriendlyString.NJ,
            State.NM => StateFriendlyString.NM,
            State.NY => StateFriendlyString.NY,
            State.NC => StateFriendlyString.NC,
            State.ND => StateFriendlyString.ND,
            State.OH => StateFriendlyString.OH,
            State.OK => StateFriendlyString.OK,
            State.OR => StateFriendlyString.OR,
            State.PA => StateFriendlyString.PA,
            State.RI => StateFriendlyString.RI,
            State.SC => StateFriendlyString.SC,
            State.SD => StateFriendlyString.SD,
            State.TN => StateFriendlyString.TN,
            State.TX => StateFriendlyString.TX,
            State.UT => StateFriendlyString.UT,
            State.VT => StateFriendlyString.VT,
            State.VA => StateFriendlyString.VA,
            State.WA => StateFriendlyString.WA,
            State.WV => StateFriendlyString.WV,
            State.WI => StateFriendlyString.WI,
            State.WY => StateFriendlyString.WY,
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
    }
}
