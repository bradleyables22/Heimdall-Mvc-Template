# Heimdall MVC App Template

ASP.NET Core MVC starter template for Heimdall. It gives MVC applications the Heimdall interaction model while keeping controllers, Razor views, Razor partials, validation, and the normal ASP.NET Core request pipeline.

Use this template when you want server-rendered MVC pages that can still perform targeted HTML updates without introducing a SPA, JSON view-model layer, or client-side component runtime for ordinary UI workflows.

## Install

Install the template from NuGet:

```powershell
dotnet new install HeimdallFramework.Templates.MvcApp
```

Create and run an app:

```powershell
dotnet new heimdall-mvc -n MyHeimdallMvcApp
cd MyHeimdallMvcApp
dotnet run
```

## Documentation

Full documentation:

https://heimdall-framework.org

Related template:

```powershell
dotnet new install HeimdallFramework.Templates.WebApp
dotnet new heimdall-webapp -n MyHeimdallApp
```

Use the Web App template for Heimdall's fluent HTML-first app shape. Use this MVC template when Razor views, controllers, and partials should remain the main application structure.

## What You Get

- ASP.NET Core MVC configured for Heimdall
- Razor views for full pages
- Razor partials for reusable fragments and action responses
- Controller-local content invocations with `[ContentInvocation]`
- `IHeimdallMvcRenderer` for rendering Razor partials from Heimdall actions
- Native Heimdall attributes in Razor markup
- Closest-state payload examples
- Closest-form payload examples
- Server-side validation with partial replacement
- Out-of-band updates for related UI regions
- Private-topic Bifrost SSE toast delivery
- Lazy loading with visible-triggered table rows
- Bootstrap and Bootstrap Icons assets

Sample pages include:

- Home
- State / counter
- Forms / notes
- Lazy loading
- Out-of-band updates
- SSE toast delivery

## Mental Model

The MVC template keeps the same Heimdall loop:

```text
Browser event
-> Heimdall content invocation
-> MVC action method
-> Razor partial HTML
-> targeted DOM update
```

Controllers own routes and related actions. Razor views own the initial page. Razor partials own the reusable HTML that actions return.

## Core Setup

The template wires Heimdall into MVC like this:

```csharp
builder.Services.AddControllersWithViews();
builder.Services.AddCors();
builder.Services.AddAntiforgery();

builder.Services
    .AddHeimdall(options =>
    {
        options.EnableDetailedErrors = true;
    })
    .AddHeimdallMvc();

var app = builder.Build();

StaticAssets.Discover(app.Environment.WebRootPath);

app.UseAntiforgery();
app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseStaticFiles();
app.UseCors();
app.UseRouting();
app.UseAuthorization();
app.UseHeimdall();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
```

`AddHeimdallMvc()` registers the MVC rendering bridge used by content invocations to render Razor partials.

## Razor Markup

Heimdall behavior can be written directly in Razor:

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

The browser sends the closest form as the payload. The server returns HTML. Heimdall swaps the result into the configured target.

## Controller-Local Content Invocation

Content invocations can live beside the page controller they support:

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

## Out-of-Band and SSE

The template includes both response-local and stream-delivered side effects:

- OOB invocation: an action returns an `<invocation>` targeting `#toast-manager`.
- SSE / Bifrost: an action publishes toast HTML to the browser's private toast topic.

The toast topic is scoped through an HTTP-only cookie-backed channel id so the demo does not broadcast every toast to every connected browser.

## Package Versions

The template currently targets:

- `HeimdallFramework.Server` `3.0.0`
- `HeimdallFramework.Web` `3.0.0`
- `HeimdallFramework.Bootstrap` `5.0.0`
- `.NET` `net10.0`

## License

MIT
