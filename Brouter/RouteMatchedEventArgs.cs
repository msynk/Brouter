namespace Brouter;

public readonly struct RouteMatchedEventArgs
{
    public string Path { get; }

    public string Location { get; }

    public RenderFragment Fragment { get; }

    public IDictionary<string, object> Parameters { get; }

    public RouteMatchedEventArgs(string location, string path, IDictionary<string, object> parameters, RenderFragment fragment)
    {
        Location = location;
        Path = path;
        Parameters = parameters;
        Fragment = fragment;
    }
}
