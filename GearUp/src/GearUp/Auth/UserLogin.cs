namespace GearUp.Auth
{
	using System.Security.Claims;
	using System.Security.Principal;

	public class UserLogin
	{
		public static string UserUniqueId(IIdentity iid)
		{
			if (iid == null) {
				return string.Empty;	
			}
			
			var identity = (ClaimsIdentity)iid;
			var claims = identity.Claims;
			string provider = null;
			string id = null;
			foreach (var claim in claims)
			{
				if (claim.Type.StartsWith("urn:facebook"))
				{
					provider = "facebook";
					if (id != null)
					{
						return provider + ":" + id;
					}
				}

				if (claim.Type.StartsWith("urn:google"))
				{
					provider = "google";
					if (id != null)
					{
						return provider + ":" + id;
					}
				}

				if (claim.Type == ClaimTypes.NameIdentifier)
				{
					id = claim.Value;
					if (provider != null)
					{
						return provider + ":" + id;
					}
				}

			}

			return null;
		}
	}
}