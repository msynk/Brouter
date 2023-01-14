namespace Brouter;

internal class RouteContext
{
    private static readonly char[] _Separator = { '/' };

    public string Id { get; set; }
    public string Path { get; set; }
    public string[] Segments { get; }
    public bool IsPrefix { get; set; }
    public RenderFragment Fragment { get; set; }
    public IDictionary<string, object> Parameters { get; set; }
    public IDictionary<string, string[]> Constraints { get; set; }

    public RouteContext(string path)
    {
        // This is a simplification. We are assuming there are no paths like /a//b/. A proper routing
        // implementation would be more sophisticated.
        Segments = path.Trim('/').Split(_Separator, StringSplitOptions.RemoveEmptyEntries);
        // Individual segments are URL-decoded in order to support arbitrary characters, assuming UTF-8 encoding.
        for (int i = 0; i < Segments.Length; i++)
        {
            Segments[i] = Uri.UnescapeDataString(Segments[i]);
        }
    }
}
