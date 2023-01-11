namespace Brouter;

public partial class Route : ComponentBase, IDisposable
{
    private readonly string id = Guid.NewGuid().ToString();

    [Parameter] public string Path { get; set; } = "";
    [Parameter] public RenderFragment ChildContent { get; set; }
    [CascadingParameter(Name = "Brouter")] protected MsynkBrouter Router { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Router == null)
        {
            throw new InvalidOperationException("A Route markup must be nested in a Switch markup.");
        }

        Router.RegisterRoute(id, ChildContent, Path);
    }

    public void Dispose()
    {
        if (Router != null)
        {
            Router.UnregisterRoute(id);
        }
    }
}
