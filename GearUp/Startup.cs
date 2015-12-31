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

			string platform = Configuration["Platform"];

			this._logger = new LoggerFactory().AddConsole(LogLevel.Information).CreateLogger("GearUp");
			_logger.LogInformation("Creating Logger");
			services.AddSingleton<ILogger>(_logger);
			services.AddSingleton<IUserAuthenticator,UserAuthenticator>();

		
			services.AddSingleton<IAppBlobStorage, AzureBlobService>();
			//var dict = new LocalDictionary();
			//services.AddSingleton<IPartitionedKeyValueDictionary>(dict);
			services.AddSingleton<IPartitionedKeyValueDictionary, AzureTableDictionary>();
			
			services.AddMvc();

#if false


				
				mvcbuilder.Configure<MvcOptions>(options =>
			{
				options.OutputFormatters
						   .Where(f => f.Instance is JsonOutputFormatter)
						   .Select(f => f.Instance as JsonOutputFormatter)
						   .First()
						   .SerializerSettings
						   .ContractResolver = new CamelCasePropertyNamesContractResolver();

				options.Filters.Add(typeof(RequireHttpsExceptForLocalHostAttribute));
			});
#endif

			services.AddDataProtection();
			services.AddSession();
			services.AddCaching();

			services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				options.Cookies.ApplicationCookie.AccessDeniedPath = "/Home/AccessDenied";
			})
				   .AddDefaultTokenProviders();
		}

		// Configure is called after ConfigureServices is called.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
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

			// Sign-out to remove the user cookie.
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
