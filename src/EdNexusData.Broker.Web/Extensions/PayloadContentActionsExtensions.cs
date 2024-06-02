
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Web.Extensions;

// public static class PayloadContentActions
// {

//     private static readonly List<SelectListItem> payloadContentActionsSelectList;

//     static PayloadContentActions() => payloadContentActionsSelectList = GeneratePayloadContentActionsSelectList();

//     private static List<SelectListItem> GeneratePayloadContentActionsSelectList()
//     {
//         var selectList = new List<SelectListItem>();

//         foreach (PayloadContentAction action in Enum.GetValues(typeof(PayloadContentAction)))
//         {
//             selectList.Add(new SelectListItem
//             {
//                 Text = action.ToString(),
//                 Value = action.ToString()
//             });
//         }

//         return selectList;
//     }

//     public static List<SelectListItem> GetSelectList() => payloadContentActionsSelectList;
// }
