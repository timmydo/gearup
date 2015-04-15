using Microsoft.AspNet.Mvc;
using System;

namespace GearUp.Services
{
	public class RequireHttpsExceptForLocalHostAttribute : RequireHttpsAttribute
	{
		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (!filterContext.HttpContext.Request.IsSecure) //need to change to IsHttps in beta 4
			{
				var hostName = filterContext?.HttpContext?.Request?.Host.Value;

				//check for existence of a : and if there, lop it off
				if (!String.IsNullOrWhiteSpace(hostName))
				{
					var indexOfColon = hostName.IndexOf(':');
					if (indexOfColon > 0) hostName = hostName.Substring(0, indexOfColon);
				}
				//only bypass if hostname is exactly localhost

				if (hostName == null || !hostName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase)) HandleNonHttpsRequest(filterContext);
			}
		}

		protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
		{
			base.HandleNonHttpsRequest(filterContext);
		}
	}
}