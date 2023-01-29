namespace Brouter;

internal class RouteTable
{
    private readonly Dictionary<string, RouteEntry> _routes = new();
    private readonly Dictionary<string, RouteEntry> _nullRoutes = new();

    public RouteEntry Add(string id, string template, RenderFragment fragment, Type component)
    {
        if (_routes.TryGetValue(id, out RouteEntry value)) return value;

        var routeTemplate = TemplateParser.ParseTemplate(template);
        var entry = new RouteEntry(routeTemplate, fragment, component);
        _routes[id] = entry;

        return entry;
    }

    public void Remove(string id) => _routes.Remove(id);

    public RouteEntry AddNull(string id, string template, RenderFragment fragment, Type component)
    {
        if (_nullRoutes.TryGetValue(id, out RouteEntry value)) return value;

        var routeTemplate = TemplateParser.ParseTemplate(template);
        var entry = new RouteEntry(routeTemplate, fragment, component);
        _nullRoutes[id] = entry;

        return entry;
    }

    public void RemoveNull(string id) => _nullRoutes.Remove(id);

    public void FindMatch(RouteContext routeContext)
    {
        foreach (var (key, routeEntry) in _routes)
        {
            routeEntry.Match(routeContext);

            if (routeContext.Fragment is not null || routeContext.Component is not null)
            {
                routeContext.Id = key;
                return;
            }
        }

        // TODO: find the best match based on nested routes
        foreach (var (key, routeEntry) in _nullRoutes)
        {
            routeEntry.Match(routeContext);

            if (routeContext.Fragment is not null || routeContext.Component is not null)
            {
                routeContext.Id = key;
                return;
            }
        }
    }

    public static void Match(RouteContext routeContext, string id, RouteEntry routeEntry)
    {
        routeEntry.Match(routeContext);

        if (routeContext.Fragment is not null || routeContext.Component is not null)
        {
            routeContext.Id = id;
        }
    }
}
