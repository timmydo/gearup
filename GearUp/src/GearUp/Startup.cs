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
using Microsoft.Framework.OptionsModel;
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
				ImagesContainer = Configuration.Get("ImagesContainer"),
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
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
		{
			// Add the following to the request pipeline only in development environment.
			if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
			{
				//app.UseBrowserLink();
				//app.UseErrorPage(ErrorPageOptions.ShowAll);
				app.Use(next => new SimpleErrorPage(next, loggerfactory.Create("SimpleErrorPage")).Invoke);
				app.UseDatabaseErrorPage(DatabaseErrorPageOptions.ShowAll);
			}
			else
			{
				// Add Error handling middleware which catches all application specific errors and
				// send the request to the following path or controller action.
				app.UseErrorHandler("/Home/Error");
			}


			// Add static files to the request pipeline.
			app.UseStaticFiles();

			// Add cookie-based authentication to the request pipeline.
			app.UseIdentity();

			app.UseCookieAuthentication(options =>
			{
				options.LoginPath = new PathString(Configuration.Get("LoginPath"));
			});

			ExternalLogin.Setup(app, Configuration);

			// Add MVC to the request pipeline.
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller}/{action}/{id?}",
					defaults: new { controller = "Home", action = "Index" });
			});
		}
	}
}
