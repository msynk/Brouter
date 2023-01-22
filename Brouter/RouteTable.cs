namespace Brouter;

internal class RouteTable
{
    private readonly Dictionary<string, RouteEntry> routes = new();

    public RouteEntry Add(string id, string template, RenderFragment fragment)
    {
        if (routes.TryGetValue(id, out RouteEntry value)) return value;

        var routeTemplate = TemplateParser.ParseTemplate(template);
        var entry = new RouteEntry(routeTemplate, fragment);
        routes[id] = entry;

        return entry;
    }

    public void Remove(string id) => routes.Remove(id);

    internal void FindMatch(RouteContext routeContext)
    {
        foreach (var (key, routeEntry) in routes)
        {
            routeEntry.Match(routeContext);

            if (routeContext.Fragment is not null)
            {
                routeContext.Id = key;
                return;
            }
        }
    }

    internal static void Match(RouteContext routeContext, string id, RouteEntry routeEntry)
    {
        routeEntry.Match(routeContext);

        if (routeContext.Fragment is not null)
        {
            routeContext.Id = id;
        }
    }
}
