using Microsoft.AspNetCore.Components.Rendering;

namespace Brouter;

public partial class Route : ComponentBase, IDisposable
{
    internal readonly string Id = Guid.NewGuid().ToString();

    [Parameter] public string Template { get; set; }
    [Parameter] public string RedirectTo { get; set; }
    [Parameter] public Type Component { get; set; }
    [Parameter] public RenderFragment Content { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    [CascadingParameter(Name = "Brouter")] protected SBrouter Brouter { get; set; }
    [CascadingParameter(Name = "ParentRoute")] internal Route Parent { get; set; }


    internal string FullTemplate => (Parent is null || string.IsNullOrWhiteSpace(Parent.FullTemplate))
                                        ? Template
                                        : $"{Parent.FullTemplate}/{Template}".Replace("//", "/");

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Brouter == null)
            throw new InvalidOperationException("A Route must be nested in a Brouter.");

        Brouter.RegisterRoute(this);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        var seq = 0;
        builder.OpenComponent<CascadingValue<Route>>(seq++);
        builder.AddAttribute(seq++, "Name", "ParentRoute");
        builder.AddAttribute(seq++, "Value", this);
        builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(builder2 => builder2.AddContent(seq, ChildContent)));
        builder.CloseComponent();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing is false) return;

        Brouter?.UnregisterRoute(Id);
    }
}
