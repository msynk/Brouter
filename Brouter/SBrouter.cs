using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace Brouter;

public partial class SBrouter : ComponentBase, IDisposable
{
    private static readonly char[] _QueryOrHashStartChar = { '?', '#' };

    private bool _firstMatched;
    private RouteContext _context;
    private string _location = string.Empty;
    private RenderFragment _currentFragment;
    private Type _currentComponent;
    private IDictionary<string, object> _parameters;
    private IDictionary<string, string[]> _constraints;
    private readonly RouteTable _routeTable = new();

    [Inject] private INavigationInterception _navInterception { get; set; }
    [Inject] private NavigationManager _navManager { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public EventHandler<RouteMatchedEventArgs> OnMatch { get; set; }


    internal void RegisterRoute(string id, string template, RenderFragment fragment, Type component)
    {
        var entry = _routeTable.Add(id, template, fragment, component);

        if (_firstMatched) return;

        //_firstMatched = MatchRoute(id, entry);
    }

    internal void UnregisterRoute(string id) => _routeTable.Remove(id);

    internal void RegisterNullRoute(string id, string template, RenderFragment fragment, Type component)
    {
        var entry = _routeTable.AddNull(id, template, fragment, component);

        if (_firstMatched) return;

        //_firstMatched = MatchRoute(id, entry);
    }

    internal void UnregisterNullRoute(string id) => _routeTable.RemoveNull(id);


    protected override void OnInitialized()
    {
        _navManager.LocationChanged += LocationChanged;

        _location = _navManager.Uri;

        CreateRouteContext();

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _navInterception.EnableNavigationInterceptionAsync();

            MatchRoute();
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        new BrouterRenderer(this, builder, _parameters, _constraints, _currentFragment, _currentComponent).BuildRenderTree();
    }


    private void LocationChanged(object sender, LocationChangedEventArgs e)
    {
        _location = e.Location;
        CreateRouteContext();
        MatchRoute();
    }

    private void CreateRouteContext()
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
            RouteTable.Match(_context, id, entry);
        }

        if (_context.Fragment is null && _context.Component is null) return false;

        _parameters = _context.Parameters;
        _constraints = _context.Constraints;
        _currentFragment = _context.Fragment;
        _currentComponent = _context.Component;

        OnMatch?.Invoke(this, new RouteMatchedEventArgs(_location, _context.Template, _parameters, _context.Fragment, _context.Component));

        StateHasChanged();

        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing is false) return;

        _navManager.LocationChanged -= LocationChanged;
    }
}
