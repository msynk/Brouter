forked from https://github.com/hez2010/BlazorRouter

# Brouter
A new router for Blazor inspired by react-router and Angular router, providing declarative routing for Blazor.

## Installation
Via nuget package (coming soon):
```
dotnet add package Brouter
```
Then add `@using Brouter` in your `_Imports.razor`

## Usage
Use `<SBrouter>` and `<Route>` to config your router as shown below:

```razor
<SBrouter>
    <Route Template="/" RedirectTo="/home" />

    <Route Template="/home" Component="typeof(HomePage)" />

    <Route Template="/counter">
        <Content>
            <CounterPage InitialValue="111" />
        </Content>
    </Route>

    <Route Template="/counter/{init:int}">
        <Content><CounterPage /></Content>
    </Route>

    <Route Template="/counter/multi/{id:int:long}/{age:long:decimal:double}/{name}">
        <Content><CounterPage /></Content>
    </Route>

    <Route Template="/fetchdata" Component="typeof(FetchDataPage)" />

    <Route Template="/*/test">
        <Content><p>Test page</p></Content>
    </Route>

    <Route>
        <Content>
            <h1 class="text-danger">404</h1>
            <p>Sorry, there's nothing at this address.</p>
        </Content>
    </Route>
</SBrouter>
```

and for nested route:

```razor
<SBrouter>
    <Route Template="/" RedirectTo="/home" />

    <Route Template="/home" Component="typeof(HomePage)" />

    <Route Template="/nested-route">
        <Route Template="/{id:int}" Component="@typeof(FetchDataPage)" />
        <Route Template="/{id:int}/hello">
            <Content><FetchDataPage Value="2" /></Content>
        </Route>
        <Route Template="/{id:int}/world">
            <Content><FetchDataPage Value="3" /></Content>
        </Route>
        <Route>
            <Content>
                <h1 class="text-danger">404 - Nested</h1>
                <p>Nested: Sorry, there's nothing at this address.</p>
            </Content>
        </Route>

        <Route Template="/nested-2">
            <Route Template="/{id:int}" Component="@typeof(FetchDataPage)" />
            <Route>
                <Content>
                    <h1 class="text-danger">404 - Nested 2</h1>
                    <p>Nested - Nested: Sorry, there's nothing at this address.</p>
                </Content>
            </Route>
        </Route>
    </Route>
</SBrouter>
```

To receive the route parameters you can use either:
- the `RouteParameters` CascadingParameter to capture all route parameters
- or a named CascadingParameter for each parameter as shown below.

```csharp
[CascadingParameter(Name = "RouteParameters")] IDictionary<string, object> Parameters { get; set; }

[CascadingParameter(Name = "id")] long Id { get; set; }

protected override void OnParametersSet()
{
    if (parameterHasSet) return;

    parameterHasSet = true;

    base.OnParametersSet();

    if (Parameters is not null && Parameters.ContainsKey("id"))
    {
        var id = (int)Parameters["id"];
    }
}

```