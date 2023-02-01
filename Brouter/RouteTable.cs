namespace Brouter;

internal class RouteTable
{
    private readonly Dictionary<string, RouteEntry> _routes = new();
    private readonly Dictionary<string, RouteEntry> _nullRoutes = new();

    public RouteEntry Add(Route route)
    {
        var routes = route.Template is null ? _nullRoutes : _routes;

        if (routes.TryGetValue(route.Id, out RouteEntry value)) return value;

        var entry = new RouteEntry(TemplateParser.ParseTemplate(route.FullTemplate), route);
        routes[route.Id] = entry;

        return entry;
    }

    public void Remove(string id)
    {
        if (_routes.Remove(id)) return;

        _nullRoutes.Remove(id);
    }


    public RouteEntry FindMatch(RouteContext routeContext)
    {
        foreach (var (key, routeEntry) in _routes)
        {
            if (routeEntry.Match(routeContext))
            {
                routeContext.Id = key;
                return routeEntry;
            }
        }

        foreach (var (key, routeEntry) in _nullRoutes.Reverse())
        {
            var template = routeEntry.Route.FullTemplate;
            if (string.IsNullOrEmpty(template) || routeContext.Path.StartsWith(template))
            {
                routeContext.Id = key;
                routeContext.Route = routeEntry.Route;
                return routeEntry;
            }
        }

        return RouteEntry.Empty;
    }

    public static bool Match(RouteContext routeContext, string id, RouteEntry routeEntry)
    {
        if (routeEntry.Match(routeContext))
        {
            routeContext.Id = id;
            return true;
        }
        return false;
    }
}
