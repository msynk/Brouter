namespace Brouter;

internal class RouteTable
{
    private readonly Dictionary<string, RouteEntry> routes = new();

    public RouteEntry Add(string id, string path, RenderFragment fragment)
    {
        if (routes.ContainsKey(id)) return routes[id];

        var routePath = PathParser.ParsePath(path);
        var entry = new RouteEntry(routePath, fragment);
        routes[id] = entry;

        return entry;
    }

    public void Remove(string id)
    {
        routes.Remove(id);
    }

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

    internal void Match(RouteContext routeContext, string id, RouteEntry routeEntry)
    {
        routeEntry.Match(routeContext);

        if (routeContext.Fragment is not null)
        {
            routeContext.Id = id;
        }
    }
}
