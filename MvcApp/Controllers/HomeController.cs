using Heimdall.Server;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using MvcApp.Models;
using System.Diagnostics;

namespace MvcApp.Controllers
{
	[ContentInvocationPrefix("main")]
	public class HomeController(IHeimdallMvcRenderer _renderer) : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
		[ContentInvocation("load-body")]
		public async ValueTask<IHtmlContent> RenderBodyAsync() => await _renderer.PartialAsync("~/Views/Home/_Body.cshtml");

	}
}
