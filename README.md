# Heimdall MVC App Template

This repository contains the ASP.NET Core MVC starter template for Heimdall. It demonstrates how to use Heimdall with controllers, Razor views, Razor partials, content invocations, safe DOM swaps, out-of-band updates, and Bifrost SSE without moving the UI workflow into a SPA.

The template application components included in this repository are examples. Keep the pieces that fit your app and remove the rest.

Feedback is welcome:

https://github.com/bradleyables22/Heimdall

---

## How To Install

Install the MVC template from NuGet:

```powershell
dotnet new install HeimdallFramework.Templates.MvcApp
```

Create a new MVC app:

```powershell
dotnet new heimdall-mvc -n MyHeimdallMvcApp
```

Run it:

```powershell
cd MyHeimdallMvcApp
dotnet run
```

The original fluent HTML-first starter is a separate template:

```powershell
dotnet new install HeimdallFramework.Templates.WebApp
dotnet new heimdall-webapp -n MyHeimdallApp
```

---

## Full Documentation Website

https://heimdall-framework.org

---

## What This Template Includes

- ASP.NET Core MVC configured for Heimdall
- Razor views and partials for page and fragment markup
- Page controllers with related content invocations
- `IHeimdallMvcRenderer` usage for rendering partials from Heimdall actions
- Native Heimdall attributes in Razor markup
- Closest-state payload examples
- Closest-form payload examples
- Server-side validation with partial replacement
- Out-of-band updates for related UI regions
- Private-topic Bifrost SSE for layout-level toasts
- Lazy-loaded table rows using visible triggers
- Bootstrap and Bootstrap Icons assets

The sample pages mirror the non-MVC Heimdall template:

- Home
- State / counter
- Forms / notes
- Lazy loading
- Out-of-band updates
- SSE toast delivery

---

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

That lets you build interactive MVC pages without adding a separate JSON API and client-side rendering layer for ordinary UI workflows.

---

## Project Shape

```text
MvcApp/
  Controllers/
    HomeController.cs
    StateController.cs
    FormsController.cs
    LazyController.cs
    OutOfBandController.cs
  Models/
  Services/
  Views/
    Home/
    State/
    Forms/
    Lazy/
    OutOfBand/
    Shared/
  wwwroot/
```

Content invocations live near the MVC page they support. For example, the counter page route and counter actions both live in `StateController`.

---

## Wire Heimdall Into MVC

The template does this for you, but an existing MVC app needs the same basic setup.

```csharp
using Heimdall.Server;
using Heimdall.Server.Rendering;

var builder = WebApplication.CreateBuilder(args);

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

---

## Use Heimdall Attributes In Razor

Heimdall does not care whether markup came from fluent HTML, a Razor view, a Razor partial, or a static file. In MVC, the clearest approach is usually to write Heimdall attributes directly in Razor.

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

The browser sends the closest form as the payload, the server action returns HTML, and Heimdall swaps the result into `#create-note-host`.

---

## Put Content Invocations Beside The Page Controller

Controller-local invocations make MVC samples easy to discover. They can use constructor injection just like normal controller actions.

```csharp
using Heimdall.Server;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

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

---

## Render Partials From Content Invocations

Use `IHeimdallMvcRenderer` when a Heimdall action should return Razor-rendered HTML.

```csharp
return await renderer.PartialAsync(
    "~/Views/Forms/_NotesList.cshtml",
    notes.GetAll());
```

Application-relative partial paths are explicit and predictable. Shared partial names also work when MVC view lookup can resolve them.

---

## Out-Of-Band Updates

Some interactions need to update more than the main target. The notes sample returns the form replacement as the main swap and also sends out-of-band invocations for the notes list and toast manager.

```csharp
var response = new HtmlContentBuilder();

response.AppendHtml(formHtml);
response.AppendHtml(HeimdallHtml.Invocation(
    targetSelector: "#notes-host",
    swap: HeimdallHtml.Swap.Inner,
    payload: notesHtml));
response.AppendHtml(HeimdallHtml.Invocation(
    targetSelector: "#toast-manager",
    swap: HeimdallHtml.Swap.AfterBegin,
    payload: toastHtml));

return response;
```

This keeps the form action server-owned while still refreshing related layout regions.

---

## SSE With Bifrost

The template includes a layout-owned toast manager that subscribes to a per-browser topic. A content invocation can publish rendered toast HTML to that topic.

```csharp
[ContentInvocation("sse")]
public async Task<IHtmlContent> Sse(HttpContext ctx)
{
    var toastHtml = await renderer.PartialAsync(
        "~/Views/Shared/_Toast.cshtml",
        new ToastItem
        {
            Header = "Hello from SSE!",
            Content = "This toast was delivered over EventSource.",
            Type = ToastType.Success
        });

    await bifrost.PublishAsync(
        ctx.GetUserToastTopic(),
        toastHtml,
        TimeSpan.FromSeconds(10));

    return new HtmlString(string.Empty);
}
```

The browser stays light: no client component model, no custom socket handler, and no JSON view model layer for this UI update.

---

## Package Versions

The template currently targets:

- `HeimdallFramework.Server` `3.0.0`
- `HeimdallFramework.Web` `3.0.0`
- `HeimdallFramework.Bootstrap` `5.0.0`
- `.NET` `net10.0`

Bootstrap is versioned independently because the helper package tracks Bootstrap itself.

---

## Building The Template Package

This repository includes a template packer project.

```powershell
dotnet build Heimdall-Mvc-Template.slnx
dotnet pack HeimdallMvcTemplatePacker\HeimdallMvcTemplatePacker.csproj -c Release -o artifacts\packages
```

The package is emitted as:

```text
artifacts/packages/HeimdallFramework.Templates.MvcApp.<version>.nupkg
```

You can test the local package before publishing:

```powershell
dotnet new install .\artifacts\packages\HeimdallFramework.Templates.MvcApp.3.0.0.nupkg
dotnet new heimdall-mvc -n SmokeTestMvcApp
dotnet build .\SmokeTestMvcApp\SmokeTestMvcApp.csproj
```

---

## License

MIT
