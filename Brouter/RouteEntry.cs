namespace Brouter;

internal class RouteEntry
{
    public readonly static RouteEntry Empty = new();
    private RouteEntry() { }


    public RouteTemplate RouteTemplate { get; }

    public Route Route { get; }


    public RouteEntry(RouteTemplate routeTemplate, Route route)
    {
        RouteTemplate = routeTemplate;
        Route = route;
    }

    internal bool Match(RouteContext routeContext)
    {
        // Empty path match all routes
        if (string.IsNullOrEmpty(RouteTemplate.Template))
        {
            if (CheckGuard(Route, routeContext) is false) return false;

            routeContext.Route = Route;
            routeContext.Template = RouteTemplate.Template;
            routeContext.Parameters = new Dictionary<string, object>();
            if (Route.Guard is null)
            {
                routeContext.RedirectTo = Route.RedirectTo;
            }
            return true;
        }

        if (RouteTemplate.TemplateSegments.Length != routeContext.Segments.Length)
        {
            if (RouteTemplate.TemplateSegments.Length == 0) return false;

            bool lastSegmentStar = RouteTemplate.TemplateSegments[^1].Value == "*" && RouteTemplate.TemplateSegments.Length - routeContext.Segments.Length == 1;
            bool lastSegmentDoubleStar = RouteTemplate.TemplateSegments[^1].Value == "**" && routeContext.Segments.Length >= RouteTemplate.TemplateSegments.Length - 1;

            if (lastSegmentStar is false && lastSegmentDoubleStar is false) return false;
        }

        // Parameters will be lazily initialized.
        IDictionary<string, object> parameters = null;
        IDictionary<string, string[]> constraints = null;

        for (int i = 0; i < RouteTemplate.TemplateSegments.Length; i++)
        {
            var templateSegment = RouteTemplate.TemplateSegments[i];
            var segment = i < routeContext.Segments.Length ? routeContext.Segments[i] : string.Empty;

            if (templateSegment.TryMatch(segment, out var matchedParameterValue) is false)
            {
                routeContext.Route = null;
                return false;
            }

            //context.Route = Route;
            //context.Template = RouteTemplate.Template;

            if (templateSegment.IsParameter)
            {
                InitParameters();
                parameters[templateSegment.Value] = matchedParameterValue;
                constraints[templateSegment.Value] = templateSegment.Constraints.Select(rc => rc.Constraint).ToArray();
            }
        }

        if (CheckGuard(Route, routeContext) is false) return false;

        routeContext.Route = Route;
        routeContext.Parameters = parameters;
        routeContext.Constraints = constraints;
        routeContext.Template = RouteTemplate.Template;

        if (Route.Guard is null)
        {
            routeContext.RedirectTo = Route.RedirectTo;
        }

        return true;

        void InitParameters()
        {
            if (parameters is null)
            {
                parameters = new Dictionary<string, object>();
                constraints = new Dictionary<string, string[]>();
            }
        };
    }

    private bool CheckGuard(Route route, RouteContext routeContext)
    {
        var result = true;

        if (route.Guard is not null)
        {
            result = route.Guard.Invoke();
            if (result is false)
            {
                routeContext.RedirectTo = route.RedirectTo;
            }
        }

        if (result && route.Parent is not null)
        {
            result = CheckGuard(route.Parent, routeContext);
        }

        return result;
    }
}
