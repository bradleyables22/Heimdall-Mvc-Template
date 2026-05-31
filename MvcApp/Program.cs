using Heimdall.Server;
using Heimdall.Server.Rendering;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
	.AddHeimdall(options => 
	{
		options.EnableDetailedErrors = true;
	})
	.AddHeimdallMvc();

builder.Services.AddAntiforgery();

var app = builder.Build();

StaticAssets.Discover(app.Environment.WebRootPath);

app.UseAntiforgery();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.UseHeimdall();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.Run();
