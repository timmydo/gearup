using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security.Cookies;
using Microsoft.Data.Entity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
//using GearUp.Models;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Security;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using GearUp.Services;

namespace GearUp
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			// Setup configuration sources.
			Configuration = new Configuration()
				.AddJsonFile("config.json")
				.AddEnvironmentVariables();
		}

		public IConfiguration Configuration { get; set; }

		// This method gets called by the runtime.
		public void ConfigureServices(IServiceCollection services, ILoggerFactory loggerfactory)
		{

			var mySettings = new SiteSettings()
			{
				BlobStorageConnectionString = Configuration.Get("BlobStorageConnectionString"),
				BlobEndpoint = Configuration.Get("BlobEndpoint"),
				DocumentDatabaseId = Configuration.Get("DocumentDatabaseId"),
				DocumentCollectionId = Configuration.Get("DocumentCollectionId"),
				DocumentEndpoint = Configuration.Get("DocumentEndpoint"),
				DocumentKey = Configuration.Get("DocumentKey"),
			};

			services.AddInstance<SiteSettings>(mySettings);

			loggerfactory.AddConsole();
			var logger = loggerfactory.Create(typeof(Startup).FullName);
			logger.WriteInformation("Creating Logger");
			services.AddInstance<ILogger>(logger);

			services.AddSingleton<DocumentDB>();

			services.AddMvc().Configure<MvcOptions>(options =>
			{
				options.OutputFormatters
						   .Where(f => f.Instance is JsonOutputFormatter)
						   .Select(f => f.Instance as JsonOutputFormatter)
						   .First()
						   .SerializerSettings
						   .ContractResolver = new CamelCasePropertyNamesContractResolver();
			});

			services.AddDataProtection();


			services.Configure<ExternalAuthenticationOptions>(options =>
			{
				options.SignInAsAuthenticationType = CookieAuthenticationDefaults.AuthenticationType;
			});

		}

		// Configure is called after ConfigureServices is called.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			// Add the following to the request pipeline only in development environment.
			if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
			{
				//app.UseBrowserLink();
				app.UseErrorPage(ErrorPageOptions.ShowAll);
				app.UseDatabaseErrorPage(DatabaseErrorPageOptions.ShowAll);
			}
			else
			{
				// Add Error handling middleware which catches all application specific errors and
				// send the request to the following path or controller action.
				app.UseErrorHandler("/Home/Error");
			}

			//app.UseServices(services =>
			//{
			//});

			// Add static files to the request pipeline.
			app.UseStaticFiles();

			// Add cookie-based authentication to the request pipeline.
			app.UseIdentity();

			app.UseCookieAuthentication(options =>
			{
				options.LoginPath = new PathString("/login");
			});


			app.UseGoogleAuthentication(options =>
			{
				options.ClientId = "332102071004-38tstqp212sbssfgkn5fil5s2qhs9upl.apps.googleusercontent.com";
				options.ClientSecret = "1HBAvwhVIeZF6Jo0O71XeJud";
			});

			app.UseFacebookAuthentication(options =>
			{
				options.ClientId = "1524415717839570";
				options.ClientSecret = "ddce01745ef5275c31eea6c2c1b9dea8";
			});

			// Choose an authentication type
			app.Map("/login", signoutApp =>
			{
				signoutApp.Run(async context =>
				{
					string authType = context.Request.Query["authtype"];
					if (!string.IsNullOrEmpty(authType))
					{
						// By default the client will be redirect back to the URL that issued the challenge (/login?authtype=foo),
						// send them to the home page instead (/).
						context.Response.Challenge(new AuthenticationProperties() { RedirectUri = "/" }, authType);
						return;
					}

					context.Response.ContentType = "text/html";
					await context.Response.WriteAsync("<html><body>");
					await context.Response.WriteAsync("Choose an authentication type: <br>");
					foreach (var type in context.GetAuthenticationTypes())
					{
						await context.Response.WriteAsync("<a href=\"?authtype=" + type.AuthenticationType + "\">" + (type.Caption ?? "(suppressed)") + "</a><br>");
					}
					await context.Response.WriteAsync("</body></html>");
				});
			});

			// Sign-out to remove the user cookie.
			app.Map("/logout", signoutApp =>
			{
				signoutApp.Run(async context =>
				{
					context.Response.SignOut(CookieAuthenticationDefaults.AuthenticationType);
					context.Response.ContentType = "text/html";
					await context.Response.WriteAsync("<html><body>");
					await context.Response.WriteAsync("You have been logged out. Goodbye " + context.User.Identity.Name + "<br>");
					await context.Response.WriteAsync("<a href=\"/\">Home</a>");
					await context.Response.WriteAsync("</body></html>");
				});
			});

			app.Map("/me", meApp =>
			{
				meApp.Run(async context =>
				{
					context.Response.ContentType = "text/html";
					await context.Response.WriteAsync("<html><body>");
					await context.Response.WriteAsync("Hello " + context.User.Identity.Name + "<br>");
					foreach (var claim in context.User.Claims)
					{
						await context.Response.WriteAsync(claim.Type + ": " + claim.Value + "<br>");
					}
					await context.Response.WriteAsync("<a href=\"/logout\">Logout</a>");
					await context.Response.WriteAsync("</body></html>");
				});
			});

			// Add MVC to the request pipeline.
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller}/{action}/{id?}",
					defaults: new { controller = "Home", action = "Index" });

				// Uncomment the following line to add a route for porting Web API 2 controllers.
				// routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
			});
		}
	}
}
