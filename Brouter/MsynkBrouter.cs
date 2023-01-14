using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace Brouter;

public partial class MsynkBrouter : ComponentBase, IDisposable
{
    private static readonly char[] _QueryOrHashStartChar = { '?', '#' };

    private bool _firstMatched;
    private RouteContext _context;
    private string _location = string.Empty;
    private RenderFragment _currentFragment;
    private IDictionary<string, object> _parameters;
    private IDictionary<string, string[]> _constraints;
    private readonly RouteTable _routeTable = new();

    [Inject] private INavigationInterception _navInterception { get; set; }
    [Inject] private NavigationManager _navManager { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public EventHandler<RouteMatchedEventArgs> OnMatch { get; set; }


    public void RegisterRoute(string id, RenderFragment fragment, string path)
    {
        var entry = _routeTable.Add(id, path, fragment);
        if (_firstMatched is false)
        {
            _firstMatched = MatchRoute(id, entry);
        }
    }

    public void UnregisterRoute(string id)
    {
        _routeTable.Remove(id);
    }


    protected override void OnInitialized()
    {
        _location = _navManager.Uri;
        _navManager.LocationChanged += LocationChanged;
        UpdateRouteContext();

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _navInterception.EnableNavigationInterceptionAsync();
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        new BrouterRenderer(this, builder, _parameters, _constraints, _currentFragment).BuildRenderTree();
    }


    private void LocationChanged(object sender, LocationChangedEventArgs e)
    {
        _location = e.Location;
        UpdateRouteContext();
        MatchRoute();
    }

    private void UpdateRouteContext()
    {
        var path = _navManager.ToBaseRelativePath(_navManager.Uri);
        var firstIndex = path.IndexOfAny(_QueryOrHashStartChar);
        path = firstIndex < 0 ? path : path.Substring(0, firstIndex);
        _context = new RouteContext($"/{path}");
    }

    private bool MatchRoute(string id = null, RouteEntry entry = null)
    {
        if (id is null || entry is null)
        {
            _routeTable.FindMatch(_context);
        }
        else
        {
            _routeTable.Match(_context, id, entry);
        }

        if (_context.Fragment is null) return false;

        _parameters = _context.Parameters;
        _constraints = _context.Constraints;
        _currentFragment = _context.Fragment;

        OnMatch?.Invoke(this, new RouteMatchedEventArgs(_location, _context.Path, _parameters, _context.Fragment));

        StateHasChanged();

        return true;
    }

    public void Dispose()
    {
        _navManager.LocationChanged -= LocationChanged;
    }
}
