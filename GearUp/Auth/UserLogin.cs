namespace GearUp.Auth
{
	using Microsoft.AspNet.Http;
	using Microsoft.AspNet.Mvc;
	using Microsoft.Extensions.Options;
	using Models;
	using Newtonsoft.Json;
	using Shared.Interfaces;
	using System;
	using System.Security.Claims;
	using System.Security.Cryptography;
	using System.Security.Principal;

	public class UserLoginInfo : IUserLoginInfo
	{
		public DateTime Date { get; set; }
		public string UserId { get; set; }
		public string FriendlyName { get; set; }
		public string Hmac { get; set; }

	}

	public class UserAuthenticator : IUserAuthenticator
	{
		public static readonly string CookieName = "gutoken";
		private static readonly double DefaultCookieTimespanInDays = 20;
		private static readonly double MaxCookieTimespanInDays = 100;
		private byte[] _hmacKey;


		public UserAuthenticator(IOptions<SiteSettings> ss)
		{
			if (string.IsNullOrEmpty(ss?.Value?.CookieHmacKey))
			{
				byte[] key = new byte[64];
				using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
				{
					rng.GetBytes(key);
				}

				throw new Exception("CookieHmacKey not found in site settings. Example: " + Convert.ToBase64String(key));
			}

			_hmacKey = Convert.FromBase64String(ss.Value.CookieHmacKey);
			if (_hmacKey.Length != 64)
			{
				throw new Exception("HMAC Key in Site Settings is not 64 bits");
			}
		}

		private static string UserUniqueId(IIdentity iid)
		{
			if (iid == null)
			{
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


		public IUserLoginInfo AuthenticateUser(Controller c)
		{
			var cookies = c.Request.Cookies;
			var user = c.User;

			string cookie;
			if (cookies.TryGetValue(CookieName, out cookie))
			{
				var uli = ParseCookie(cookie);
				if (uli != null)
				{
					return uli;
				}
			}

			var id = UserUniqueId(user?.Identity);
			if (!string.IsNullOrEmpty(id))
			{
				var li = new UserLoginInfo();
				li.FriendlyName = user?.Identity?.Name ?? "";
				li.UserId = id;
				return li;
			}

			return null;
		}


		public string GenerateCookie(IUserLoginInfo userLoginInfo)
		{
			userLoginInfo.Date = DateTime.UtcNow.AddDays(DefaultCookieTimespanInDays);
			userLoginInfo.Hmac = CalculateHmac(userLoginInfo);
			var str = JsonConvert.SerializeObject(userLoginInfo, Formatting.None);
			return System.Net.WebUtility.UrlEncode(str);
		}

		private UserLoginInfo ParseCookie(string cookie)
		{
			if (string.IsNullOrEmpty(cookie))
			{
				return null;
			}

			var decoded = System.Net.WebUtility.UrlDecode(cookie);
			var userLoginInfo = JsonConvert.DeserializeObject<UserLoginInfo>(decoded);
			if (!VerifyLoginInfo(userLoginInfo))
			{
				return null;
			}

			return userLoginInfo;
		}


		private string CalculateHmac(IUserLoginInfo userLoginInfo)
		{
			var encoding = new System.Text.ASCIIEncoding();
			var bytes = encoding.GetBytes(userLoginInfo.Date.ToString("u") + userLoginInfo.UserId);

			using (var hmacsha256 = new HMACSHA256(_hmacKey))
			{
				byte[] hashmessage = hmacsha256.ComputeHash(bytes);
				return Convert.ToBase64String(hashmessage);
			}
		}
		

		public bool VerifyLoginInfo(IUserLoginInfo userLoginInfo)
		{
			// make sure the date is in the future, but not too far in the future
			var now = DateTime.UtcNow;
			if (now > userLoginInfo.Date || now.AddDays(MaxCookieTimespanInDays) < userLoginInfo.Date)
			{
				return false;
			}

			if (CalculateHmac(userLoginInfo) == userLoginInfo.Hmac)
			{
				return true;
			}

			return false;
		}
	}
}