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


    internal void RegisterRoute(Route route)
    {
        var entry = _routeTable.Add(route);

        if (_firstMatched) return;

        //_firstMatched = Match(id, entry);
    }

    internal void UnregisterRoute(string id) => _routeTable.Remove(id);


    protected override void OnInitialized()
    {
        _navManager.LocationChanged += LocationChanged;

        _location = _navManager.Uri;

        CreateRouteContext();

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender is false) return;
        await _navInterception.EnableNavigationInterceptionAsync();

        if (FindMatch() is false) return;

        UpdateView();
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

        if (FindMatch() is false) return;

        UpdateView();
    }

    private void CreateRouteContext()
    {
        var path = _navManager.ToBaseRelativePath(_navManager.Uri);
        var firstIndex = path.IndexOfAny(_QueryOrHashStartChar);

        path = firstIndex < 0 ? path : path.Substring(0, firstIndex);

        _context = new RouteContext($"/{path}");
    }

    private bool FindMatch()
    {
        var foundRouteEntry = _routeTable.FindMatch(_context);

        if (string.IsNullOrEmpty(foundRouteEntry.Route.RedirectTo) is false)
        {
            _navManager.NavigateTo(foundRouteEntry.Route.RedirectTo);
            return true;
        }

        return foundRouteEntry != RouteEntry.Empty;
    }

    private bool Match(string id, RouteEntry routeEntry)
    {

        return RouteTable.Match(_context, id, routeEntry);
    }

    private void UpdateView()
    {
        if (_context.Route.Content is null && _context.Route.Component is null) return;

        _parameters = _context.Parameters;
        _constraints = _context.Constraints;
        _currentFragment = _context.Route.Content;
        _currentComponent = _context.Route.Component;

        OnMatch?.Invoke(this, new RouteMatchedEventArgs(_location, _context.Template, _parameters, _context.Route.Content, _context.Route.Component));

        StateHasChanged();
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
