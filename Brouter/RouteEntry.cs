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

        for (int i = 0; i < RoutePath.Segments.Length; i++)
        {
            var segment = RoutePath.Segments[i];
            var pathSegment = i < context.Segments.Length ? context.Segments[i] : string.Empty;

            if (segment.TryMatch(pathSegment, out var matchedParameterValue) is false)
            {
                context.Fragment = null;
                return;
            }

            context.Fragment = Fragment;
            context.Path = RoutePath.Path;

            if (segment.IsParameter)
            {
                GetParameters()[segment.Value] = matchedParameterValue;
            }
        }

        context.Fragment = Fragment;
        context.Path = RoutePath.Path;
        context.Parameters = parameters;

        IDictionary<string, object> GetParameters() => (parameters = parameters == null ? new Dictionary<string, object>() : parameters);
    }

    private bool IsContextInvalid(RouteContext context)
    {
        bool segmentsMatch = RoutePath.Segments.Length != context.Segments.Length;
        bool zeroSegments = RoutePath.Segments.Length == 0;
        bool lastSegmentStar = RoutePath.Segments[^1].Value == "*" && RoutePath.Segments.Length - context.Segments.Length == 1;
        bool lastSegmentDoubleStars = (RoutePath.Segments[^1].Value == "**" && context.Segments.Length >= RoutePath.Segments.Length - 1);

        return segmentsMatch && (zeroSegments || (lastSegmentStar is false && lastSegmentDoubleStars is false));
    }
}
