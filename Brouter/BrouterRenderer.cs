using Microsoft.AspNetCore.Components.Rendering;

namespace Brouter;

internal class BrouterRenderer
{
    private readonly MsynkBrouter _brouter;
    private readonly RenderTreeBuilder _builder;
    private readonly RenderFragment _currentFragment;
    private readonly IDictionary<string, object> _parameters;
    private readonly IDictionary<string, string[]> _constraints;

    public BrouterRenderer(
                    MsynkBrouter brouter,
                    RenderTreeBuilder builder,
                    IDictionary<string, object> parameters,
                    IDictionary<string, string[]> constraints,
                    RenderFragment currentFragment)
    {
        _brouter = brouter;
        _builder = builder;
        _parameters = parameters;
        _constraints = constraints;
        _currentFragment = currentFragment;
    }

    public void BuildRenderTree()
    {
        var seq = CreateBrouterCascadingValue();
        CreateParametersCascadingValues(++seq);
    }

    private int CreateBrouterCascadingValue()
    {
        var seq = 0;
        _builder.OpenComponent<CascadingValue<MsynkBrouter>>(seq++);
        _builder.AddAttribute(seq++, "Name", "Brouter");
        _builder.AddAttribute(seq++, "Value", _brouter);
        _builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(builder2 => builder2.AddContent(seq, _brouter.ChildContent)));
        _builder.CloseComponent();
        return seq;
    }

    private void CreateParametersCascadingValues(int seq)
    {
        if (_parameters is null)
        {
            AddRouteParams(_builder, seq);
            return;
        };

        RecursiveCreate(_builder, 0, seq, _parameters.ToArray());
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
        builder.OpenComponent<CascadingValue<IDictionary<string, object>>>(seq++);
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


}