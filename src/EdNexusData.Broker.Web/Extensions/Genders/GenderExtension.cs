
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Web.Extensions.Genders;

public static class GenderFriendlyString
{
    public const string Female = "Female";
    public const string Male = "Male";
    public const string Nonbinary = "Nonbinary";
}

public static class Genders
{

    private static readonly List<SelectListItem> genderSelectList;

    static Genders() => genderSelectList = GenerateGenderSelectList();

    private static List<SelectListItem> GenerateGenderSelectList()
    {
        var selectList = new List<SelectListItem>();

        foreach (Gender gender in Enum.GetValues(typeof(Gender)))
        {
            var friendlyGender = gender.ToFriendlyString();
            selectList.Add(new SelectListItem
            {
                Text = friendlyGender,
                Value = gender.ToString()
            });
        }

        return selectList;
    }

    public static List<SelectListItem> GetSelectList() => genderSelectList;

    public static string ToFriendlyString(this Gender gender)
    {
        return gender switch
        {
            Gender.Female => GenderFriendlyString.Female,
            Gender.Male => GenderFriendlyString.Male,
            Gender.Nonbinary => GenderFriendlyString.Nonbinary,
            _ => throw new ArgumentOutOfRangeException(nameof(gender))
        };
    }
}
