namespace EdNexusData.Broker.Web.Extensions.Routes
{
	public static class RouteExtension
	{
        public static void MapControllerRoutes(this IEndpointRouteBuilder endpoints, string routeName, string controllerName)
        {
            endpoints.MapControllerRoute(
                name: $"{routeName}Index",
                pattern: $"{routeName}",
                defaults: new { controller = controllerName, action = "Index" });

            endpoints.MapControllerRoute(
                name: $"{routeName}Create",
                pattern: $"{routeName}/add",
                defaults: new { controller = controllerName, action = "Create" });

            endpoints.MapControllerRoute(
                name: $"{routeName}Update",
                pattern: $"{routeName}/edit",
                defaults: new { controller = controllerName, action = "Update" });

            endpoints.MapControllerRoute(
                name: $"{routeName}Delete",
                pattern: $"{routeName}/delete",
                defaults: new { controller = controllerName, action = "Delete" });
        }
    }
}
