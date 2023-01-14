namespace Brouter;

internal class RouteEntry
{
    public RoutePath RoutePath { get; }

    public RenderFragment Fragment { get; }

    public RouteEntry(RoutePath routePath, RenderFragment fragment)
    {
        RoutePath = routePath;
        Fragment = fragment;
    }

    internal void Match(RouteContext context)
    {
        // Empty path match all routes
        if (string.IsNullOrEmpty(RoutePath.Path))
        {
            context.Parameters = new Dictionary<string, object>();
            context.Fragment = Fragment;
            context.Path = RoutePath.Path;
            return;
        }

        if (RoutePath.Segments.Length != context.Segments.Length)
        {
            if (RoutePath.Segments.Length == 0) return;

            bool lastSegmentStar = RoutePath.Segments[^1].Value == "*" && RoutePath.Segments.Length - context.Segments.Length == 1;
            bool lastSegmentDoubleStar = RoutePath.Segments[^1].Value == "**" && context.Segments.Length >= RoutePath.Segments.Length - 1;

            if (lastSegmentStar is false && lastSegmentDoubleStar is false) return;
        }

        // Parameters will be lazily initialized.
        IDictionary<string, object> parameters = null;
        IDictionary<string, string[]> constraints = null;

        for (int i = 0; i < RoutePath.Segments.Length; i++)
        {
            var pathSegment = RoutePath.Segments[i];
            var segment = i < context.Segments.Length ? context.Segments[i] : string.Empty;

            if (pathSegment.TryMatch(segment, out var matchedParameterValue) is false)
            {
                context.Fragment = null;
                return;
            }

            context.Fragment = Fragment;
            context.Path = RoutePath.Path;

            if (pathSegment.IsParameter)
            {
                InitParameters();
                parameters[pathSegment.Value] = matchedParameterValue;
                constraints[pathSegment.Value] = pathSegment.Constraints.Select(rc => rc.Constraint).ToArray();
            }
        }

        context.Fragment = Fragment;
        context.Path = RoutePath.Path;
        context.Parameters = parameters;
        context.Constraints = constraints;

        void InitParameters()
        {
            if (parameters is null)
            {
                parameters = new Dictionary<string, object>();
                constraints = new Dictionary<string, string[]>();
            }
        };
    }
}
