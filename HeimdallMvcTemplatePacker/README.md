# Heimdall MVC App Template

Alpha status

Heimdall is currently in alpha. APIs, naming, and patterns may change as the project evolves.

This is the ASP.NET Core MVC starter template for Heimdall. It demonstrates how to use Heimdall with controllers, Razor views, Razor partials, content invocations, safe DOM swaps, and optional SSE without moving ordinary UI workflows into a SPA.

## Install

```powershell
dotnet new install HeimdallFramework.Templates.MvcApp
dotnet new heimdall-mvc -n MyHeimdallMvcApp
cd MyHeimdallMvcApp
dotnet run
```

## Documentation

https://heimdall-framework.org

## What You Get

- ASP.NET Core MVC configured for Heimdall
- Razor views and partials for page and fragment markup
- Page controllers with related content invocations
- `IHeimdallMvcRenderer` usage for rendering partials from Heimdall actions
- Native Heimdall attributes in Razor markup
- Closest-state payload examples
- Closest-form payload examples
- Server-side validation with partial replacement
- Out-of-band updates for related UI regions
- Optional Bifrost SSE for layout-level toasts
- Lazy-loaded table rows
- Bootstrap and Bootstrap Icons assets

## Why Use The MVC Template

The original Heimdall template is a pure HTML-first app built around explicit page render functions and fluent HTML composition.

This template is for teams that want the same Heimdall interaction model while keeping an MVC application shape:

- controllers own routes
- Razor views own initial page markup
- Razor partials own reusable fragments
- content invocations return rendered partial HTML
- the browser applies server-rendered updates through Heimdall attributes

The model is still:

```text
Event -> Content Invocation -> Razor Partial HTML -> Targeted DOM update
```

## Core Setup

```csharp
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery();

builder.Services
    .AddHeimdall(options =>
    {
        options.EnableDetailedErrors = true;
    })
    .AddHeimdallMvc();

var app = builder.Build();

app.UseAntiforgery();
app.MapStaticAssets();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseHeimdall();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
```

`AddHeimdallMvc()` registers the MVC rendering bridge used by content invocations to render Razor partials.

## Razor Markup

```cshtml
<form id="create-note-form"
      heimdall-content-submit="notes.create"
      heimdall-payload-from="closest-form"
      heimdall-content-target="#create-note-host"
      heimdall-content-swap="inner"
      heimdall-prevent-default="true">
    <label class="form-label" for="title">Title</label>
    <input id="title" name="Title" class="form-control" />

    <button class="btn btn-primary" type="submit">Submit</button>
</form>
```

## Controller-Local Content Invocation

```csharp
[ContentInvocationPrefix("counter")]
public sealed class StateController(IHeimdallMvcRenderer renderer) : Controller
{
    [Route("state")]
    public IActionResult Index()
    {
        return View(new CounterState());
    }

    [ContentInvocation("inc")]
    public Task<IHtmlContent> Increment([ContentPayload] CounterState? state)
    {
        var next = new CounterState { Count = (state?.Count ?? 0) + 1 };
        return renderer.PartialAsync("~/Views/State/_CounterHost.cshtml", next);
    }
}
```

The invocation id is `counter.inc`.

## License

MIT
