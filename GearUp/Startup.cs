namespace GearUp
{
	using System;
	using GearUp.Services;
	using GearUp.Interfaces;
	using Microsoft.AspNet.Builder;
	using Microsoft.AspNet.Hosting;
	using Microsoft.AspNet.Http;
	using Microsoft.AspNet.Http.Authentication;
	using Microsoft.AspNet.Identity.EntityFramework;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Microsoft.Extensions.PlatformAbstractions;
	using Shared.Interfaces;
	using Models;
	using Auth;
	using Microsoft.AspNet.HttpOverrides;
	using System.Linq;

	public class ApplicationUser : IdentityUser { }

	public class Startup
	{
		private ILogger _logger;

		public Startup(IApplicationEnvironment applicationEnvironment)
		{
			Console.WriteLine("Application startup");

			var builder = new ConfigurationBuilder()
				.SetBasePath(applicationEnvironment.ApplicationBasePath)
				.AddJsonFile("config.json")
				.AddUserSecrets()
				.AddEnvironmentVariables();
			
			Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; set; }

		// This method gets called by the runtime.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<SiteSettings>(Configuration.GetSection("AppSettings"));

			this._logger = new LoggerFactory().AddConsole(LogLevel.Information).CreateLogger("GearUp");
			_logger.LogInformation("Creating Logger");
			services.AddSingleton<ILogger>(_logger);
			services.AddSingleton<IUserAuthenticator,UserAuthenticator>();

			services.AddSingleton<IAppBlobStorage, AzureBlobService>();
			services.AddSingleton<IPartitionedKeyValueDictionary, AzureTableDictionary>();
			
			services.AddMvc();
			services.AddDataProtection();
			services.AddSession();
			services.AddCaching();

			services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				options.Cookies.ApplicationCookie.AccessDeniedPath = "/Home/AccessDenied";
			}).AddDefaultTokenProviders();
		}

		// Configure is called after ConfigureServices is called.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseOverrideHeaders(config => config.ForwardedOptions = ForwardedHeaders.All);
			//app.UseRequestLogger("post-override");

			app.UseIISPlatformHandler();

			if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
			{
				app.UseDeveloperExceptionPage();
				app.UseRuntimeInfoPage();
			}
			else
			{
				app.UseDeveloperExceptionPage();
				app.UseRuntimeInfoPage();
			}

			app.UseSession();
			app.UseStaticFiles();
			app.UseIdentity();

			app.UseCookieAuthentication(options =>
			{
				options.LoginPath = new PathString("/login");
			});

			app.UseGoogleAuthentication(options =>
			{
				options.ClientId = Configuration["OAuth:GoogleClientId"];
				options.ClientSecret = Configuration["OAuth:GoogleClientSecret"];
			});

			app.UseFacebookAuthentication(options =>
			{
				options.ClientId = Configuration["OAuth:FacebookClientId"];
				options.ClientSecret = Configuration["OAuth:FacebookClientSecret"];
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
						await context.Authentication.ChallengeAsync(authType, new AuthenticationProperties() { RedirectUri = "/" });
						return;
					}

					context.Response.ContentType = "text/html";
					await context.Response.WriteAsync("<html><body>");
					await context.Response.WriteAsync("Choose an authentication type: <br>");
					foreach (var type in context.Authentication.GetAuthenticationSchemes())
					{
						await context.Response.WriteAsync("<a href=\"?authtype=" + type.AuthenticationScheme + "\">" + (type.DisplayName ?? "(suppressed)") + "</a><br>");
					}
					await context.Response.WriteAsync("</body></html>");
				});
			});

			app.Map("/logout", signoutApp =>
			{
				signoutApp.Run(async context =>
				{
					await context.Authentication.SignOutAsync("OpenIdConnect");
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
