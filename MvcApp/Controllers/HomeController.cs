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
		[Route("")]
		public IActionResult Index()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
			});
		}

		[ContentInvocation("body")]
		public async ValueTask<IHtmlContent> RenderBodyAsync() => await _renderer.PartialAsync("~/Views/Home/_Body.cshtml");

	}
}
