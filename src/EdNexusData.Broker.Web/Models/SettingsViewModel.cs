using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Connector;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Web.Models;

public class SettingsViewModel
{
    public List<Type>? ConnectorTypes { get; set; }

    public List<Type>? PayloadTypes { get; set; }

    public List<dynamic>? Models { get; set; } = new List<dynamic>();
}