namespace Brouter;

internal class RoutePath
{
    public static readonly char[] Separators = { '/' };

    public string Path { get; }

    public PathSegment[] Segments { get; }

    public RoutePath(string path, PathSegment[] segments)
    {
        this.Path = path;
        Segments = segments;
    }
}
