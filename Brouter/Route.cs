using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using System.IO;

namespace Brouter;

public partial class Route : ComponentBase, IDisposable
{
    private readonly string id = Guid.NewGuid().ToString();

    [Parameter] public string Template { get; set; }
    [Parameter] public Type Component { get; set; }
    [Parameter] public RenderFragment Content { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    [CascadingParameter(Name = "Brouter")] protected SBrouter Brouter { get; set; }
    [CascadingParameter(Name = "NestedTemplate")] protected string NestedTemplate { get; set; }


    private string InternalTemplate => string.IsNullOrWhiteSpace(NestedTemplate) ? Template : $"{NestedTemplate}/{Template}".Replace("//", "/");

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Brouter == null)
            throw new InvalidOperationException("A Route must be nested in a Brouter.");

        if (Template is null)
        {
            Brouter.RegisterNullRoute(id, InternalTemplate, Content, Component);
        }
        else
        {
            Brouter.RegisterRoute(id, InternalTemplate, Content, Component);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        var seq = 0;
        builder.OpenComponent<CascadingValue<string>>(seq++);
        builder.AddAttribute(seq++, "Name", "NestedTemplate");
        builder.AddAttribute(seq++, "Value", InternalTemplate);
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

        Brouter?.UnregisterRoute(id);
    }
}
