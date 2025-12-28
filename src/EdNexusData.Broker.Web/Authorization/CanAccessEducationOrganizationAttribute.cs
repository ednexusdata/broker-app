namespace EdNexusData.Broker.Web.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)] 
public class CanAccessEducationOrganizationAttribute : Attribute { }