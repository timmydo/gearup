using Microsoft.Framework.ConfigurationModel;
using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Security.Cookies;

namespace GearUp.Services
{
	public class ExternalLogin
	{

		public static void Setup(IApplicationBuilder app, IConfiguration Configuration)
		{

			app.UseGoogleAuthentication(options =>
			{
				options.ClientId = Configuration.Get("GoogleClientId");
				options.ClientSecret = Configuration.Get("GoogleClientSecret");
			});

			app.UseFacebookAuthentication(options =>
			{
				options.ClientId = Configuration.Get("FacebookClientId");
				options.ClientSecret = Configuration.Get("FacebookClientSecret");
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
		}
	}
}