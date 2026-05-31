using Heimdall.Server;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace MvcApp.Controllers
{
	[ContentInvocationPrefix("layout")]
	public class LayoutController(IHeimdallMvcRenderer _renderer) : Controller
	{
		[ContentInvocation("load-header")]
		public async ValueTask<IHtmlContent> LoadHeader() => await _renderer.PartialAsync("~/Views/Shared/_Header.cshtml");
	}
}
