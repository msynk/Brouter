namespace Brouter;

internal class RouteEntry
{
    public RouteTemplate RouteTemplate { get; }
    public RenderFragment Fragment { get; }
    public Type Component { get; }

    public RouteEntry(RouteTemplate routeTemplate, RenderFragment fragment, Type component)
    {
        RouteTemplate = routeTemplate;
        Fragment = fragment;
        Component = component;
    }

    internal void Match(RouteContext context)
    {
        // Empty path match all routes
        if (string.IsNullOrEmpty(RouteTemplate.Template))
        {
            context.Parameters = new Dictionary<string, object>();
            context.Template = RouteTemplate.Template;
            context.Fragment = Fragment;
            context.Component = Component;

            return;
        }

        if (RouteTemplate.TemplateSegments.Length != context.Segments.Length)
        {
            if (RouteTemplate.TemplateSegments.Length == 0) return;

            bool lastSegmentStar = RouteTemplate.TemplateSegments[^1].Value == "*" && RouteTemplate.TemplateSegments.Length - context.Segments.Length == 1;
            bool lastSegmentDoubleStar = RouteTemplate.TemplateSegments[^1].Value == "**" && context.Segments.Length >= RouteTemplate.TemplateSegments.Length - 1;

            if (lastSegmentStar is false && lastSegmentDoubleStar is false) return;
        }

        // Parameters will be lazily initialized.
        IDictionary<string, object> parameters = null;
        IDictionary<string, string[]> constraints = null;

        for (int i = 0; i < RouteTemplate.TemplateSegments.Length; i++)
        {
            var templateSegment = RouteTemplate.TemplateSegments[i];
            var segment = i < context.Segments.Length ? context.Segments[i] : string.Empty;

            if (templateSegment.TryMatch(segment, out var matchedParameterValue) is false)
            {
                context.Fragment = null;
                context.Component = null;

                return;
            }

            //context.Template = RouteTemplate.Template;
            //context.Fragment = Fragment;
            //context.Component = Component;

            if (templateSegment.IsParameter)
            {
                InitParameters();
                parameters[templateSegment.Value] = matchedParameterValue;
                constraints[templateSegment.Value] = templateSegment.Constraints.Select(rc => rc.Constraint).ToArray();
            }
        }

        context.Template = RouteTemplate.Template;
        context.Fragment = Fragment;
        context.Component = Component;
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
