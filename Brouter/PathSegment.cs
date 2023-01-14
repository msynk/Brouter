namespace Brouter;

internal class PathSegment
{
    // The value of the segment. The exact text to match when is a literal.
    // The parameter name when its a segment
    public string Value { get; }

    public bool IsParameter { get; }

    public RouteConstraint[] Constraints { get; }

    public PathSegment(string path, string segment, bool isParameter)
    {
        IsParameter = isParameter;

        if (isParameter is false || segment.IndexOf(':') < 0)
        {
            Value = segment;
            Constraints = Array.Empty<RouteConstraint>();
        }
        else
        {
            var tokens = segment.Split(':');
            if (tokens[0].Length == 0)
            {
                throw new ArgumentException($"Malformed parameter '{segment}' in route '{path}' has no name before the constraints list.");
            }

            Value = tokens[0];
            Constraints = tokens.Skip(1)
                .Select(constraint => RouteConstraint.Parse(path, segment, constraint))
                .ToArray();
        }
    }

    public bool TryMatch(string pathSegment, out object matchedParameterValue)
    {
        if (IsParameter)
        {
            matchedParameterValue = pathSegment;

            foreach (var constraint in Constraints)
            {
                if (constraint.TryMatch(pathSegment, out matchedParameterValue) is false)
                {
                    return false;
                }
            }

            return true;
        }

        matchedParameterValue = null;
        return Value == "**" || Value == "*" || string.Equals(Value, pathSegment, StringComparison.OrdinalIgnoreCase);
    }
}
