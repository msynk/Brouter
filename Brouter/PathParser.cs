namespace Brouter;

// This implementation is temporary, in the future we'll want to have
// a more performant/properly designed routing set of abstractions.
// To be more precise these are some things we are scoping out:
// * We are not doing link generation.
// * We are not supporting all the route constraint formats supported by ASP.NET server-side routing.
// The class in here just takes care of parsing a route and extracting
// simple parameters from it.
// Some differences with ASP.NET Core routes are:
// * We don't support catch all parameter segments.
// * We don't support optional parameter segments.
// * We don't support complex segments.
// The things that we support are:
// * Literal path segments. (Like /Path/To/Some/Page)
// * Parameter path segments (Like /Customer/{Id}/Orders/{OrderId})
internal class PathParser
{
    public static readonly char[] InvalidParameterNameCharacters =
        { '*', '?', '{', '}', '=', '.' };

    internal static RoutePath ParsePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return new RoutePath("", Array.Empty<PathSegment>());

        var originalPath = path;
        path = path.Trim('/');
        if (path == "")
        {
            // Special case "/";
            return new RoutePath("/", Array.Empty<PathSegment>());
        }

        var segments = path.Split('/');
        var pathSegments = new PathSegment[segments.Length];
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            if (string.IsNullOrEmpty(segment))
                throw new InvalidOperationException($"Invalid path '{path}'. Empty segments are not allowed.");

            if (segment[0] != '{')
            {
                if (segment[segment.Length - 1] == '}')
                    throw new InvalidOperationException($"Invalid path '{path}'. Missing '{{' in parameter segment '{segment}'.");

                pathSegments[i] = new PathSegment(originalPath, segment, isParameter: false);
            }
            else
            {
                if (segment[segment.Length - 1] != '}')
                    throw new InvalidOperationException($"Invalid path '{path}'. Missing '}}' in parameter segment '{segment}'.");

                if (segment.Length < 3)
                    throw new InvalidOperationException($"Invalid path '{path}'. Empty parameter name in segment '{segment}' is not allowed.");

                var invalidCharacter = segment.IndexOfAny(InvalidParameterNameCharacters, 1, segment.Length - 2);
                if (invalidCharacter != -1)
                    throw new InvalidOperationException($"Invalid path '{path}'. The character '{segment[invalidCharacter]}' in parameter segment '{segment}' is not allowed.");

                pathSegments[i] = new PathSegment(originalPath, segment.Substring(1, segment.Length - 2), isParameter: true);
            }
        }

        for (int i = 0; i < pathSegments.Length; i++)
        {
            var currentSegment = pathSegments[i];
            if (currentSegment.IsParameter is false) continue;

            for (int j = i + 1; j < pathSegments.Length; j++)
            {
                var nextSegment = pathSegments[j];
                if (nextSegment.IsParameter is false) continue;

                if (string.Equals(currentSegment.Value, nextSegment.Value, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Invalid path '{path}'. The parameter '{currentSegment}' appears multiple times.");
            }
        }

        return new RoutePath(path, pathSegments);
    }
}
