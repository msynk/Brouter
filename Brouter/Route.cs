namespace Brouter;

public partial class Route : ComponentBase, IDisposable
{
    private readonly string id = Guid.NewGuid().ToString();

    [Parameter] public string Template { get; set; } = "";
    [Parameter] public RenderFragment ChildContent { get; set; }

    [CascadingParameter(Name = "Brouter")] protected SBrouter Router { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Router == null)
        {
            throw new InvalidOperationException("A Route markup must be nested in a Switch markup.");
        }

        Router.RegisterRoute(id, ChildContent, Template);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Router?.UnregisterRoute(id);
        }
    }
}
