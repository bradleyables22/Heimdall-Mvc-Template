# Heimdall MVC App Template

An ASP.NET Core MVC starter built with **Heimdall**, using Razor views and partials for HTML-first, server-driven UI updates.

This template demonstrates:

- ASP.NET Core MVC with Heimdall configured
- Razor views and partials instead of fluent HTML page composition
- Content invocations exposed from page controllers
- Safe DOM swapping with server-rendered partials
- Form validation with out-of-band updates
- Closest-state payloads
- Lazy-loaded table rows
- Optional SSE toast delivery with Bifrost

## Usage

```bash
dotnet new install HeimdallFramework.Templates.MvcApp
dotnet new heimdall-mvc -n MyHeimdallMvcApp
```

## License

MIT
