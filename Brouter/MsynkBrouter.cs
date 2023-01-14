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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        var seq = 0;
        CreateBrouterCascadingValue(this);
        CreateParametersCascadingValues(builder, seq);

        void CreateBrouterCascadingValue<TValue>(TValue value)
        {
            builder.OpenComponent<CascadingValue<TValue>>(seq++);
            builder.AddAttribute(seq++, "Name", "Brouter");
            builder.AddAttribute(seq++, "Value", value);
            builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(builder2 => builder2.AddContent(seq, ChildContent)));
            builder.CloseComponent();
        }
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

    private void CreateParametersCascadingValues(RenderTreeBuilder builder, int seq)
    {
        seq++;

        if (_parameters is null)
        {
            AddRouteParams(builder, seq);
            return;
        };

        RecursiveCreate(builder, 0, seq, _parameters.ToArray());
    }

    private void RecursiveCreate(RenderTreeBuilder builder, int idx, int seq, KeyValuePair<string, object>[] arr)
    {
        var p = arr[idx];

        seq = OpenComp(builder, p.Key, p.Value, seq);

        builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(builder2 =>
        {
            if (++idx == arr.Length)
            {
                AddRouteParams(builder2, seq);
                return;
            }

            RecursiveCreate(builder2, idx, seq, arr);
        }));

        builder.CloseComponent();
    }

    private void AddRouteParams(RenderTreeBuilder builder, int seq)
    {
        builder.OpenComponent<CascadingValue<IDictionary<string,object>>>(seq++);
        builder.AddAttribute(seq++, "Name", "RouteParameters");
        builder.AddAttribute(seq++, "Value", _parameters);
        builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(builder2 => builder2.AddContent(seq, _currentFragment)));
        builder.CloseComponent();
    }

    private int OpenComp<T>(RenderTreeBuilder builder, string name, T value, int seq)
    {
        var constraints = _constraints[name];
        if (constraints is null || constraints.Length == 0)
        {
            builder.OpenComponent<CascadingValue<T>>(seq++);
        }
        else
        {
            var constraint = constraints[0]; // TODO: improve to consider all constrains
            if (constraint is "int")
            {
                builder.OpenComponent<CascadingValue<int>>(seq++);
            }
            else if (constraint is "bool")
            {
                builder.OpenComponent<CascadingValue<bool>>(seq++);
            }
            else if (constraint is "guid")
            {
                builder.OpenComponent<CascadingValue<Guid>>(seq++);
            }
            else if (constraint is "long")
            {
                builder.OpenComponent<CascadingValue<long>>(seq++);
            }
            else if (constraint is "float")
            {
                builder.OpenComponent<CascadingValue<float>>(seq++);
            }
            else if (constraint is "double")
            {
                builder.OpenComponent<CascadingValue<double>>(seq++);
            }
            else if (constraint is "decimal")
            {
                builder.OpenComponent<CascadingValue<decimal>>(seq++);
            }
            else if (constraint is "datetime")
            {
                builder.OpenComponent<CascadingValue<DateTime>>(seq++);
            }
        }
        builder.AddAttribute(seq++, "Name", name);
        builder.AddAttribute(seq++, "Value", value);
        return seq;
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

        _currentFragment = _context.Fragment;
        _parameters = _context.Parameters;
        _constraints = _context.Constraints;

        OnMatch?.Invoke(this, new RouteMatchedEventArgs(_location, _context.Path, _parameters, _context.Fragment));

        StateHasChanged();

        return true;
    }

    public void Dispose()
    {
        _navManager.LocationChanged -= LocationChanged;
    }
}
