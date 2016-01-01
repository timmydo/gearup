using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearUp.Services
{
	public static class LogRequestMiddlewareExtensions
	{
		public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder, string name)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			return builder.Use(next => new LogRequestMiddleware(next, new LogRequestOptions()
			{
				Logger = new LoggerFactory().AddConsole(LogLevel.Information).CreateLogger(name ?? string.Empty)
			}).Invoke);
		}
	}

	public class LogRequestOptions
	{
		public ILogger Logger;
	}

    public class LogRequestMiddleware
    {
		private RequestDelegate _next;
		private LogRequestOptions _options;
		private ILogger logger;

		public LogRequestMiddleware(RequestDelegate next, LogRequestOptions options)
		{
			if (next == null) throw new ArgumentNullException(nameof(next));
			if (options == null) throw new ArgumentNullException(nameof(options));

			this._next = next;
			this._options = options;
		}

		private void Log(string s)
		{
			this._options.Logger.LogInformation(s);
		}

		public Task Invoke(HttpContext c)
		{
			c.Request.Headers.Keys.ToList().ForEach(key =>
			{
				string val = c.Request.Headers[key];
				if (!string.IsNullOrEmpty(val) && val.Length < 100)
				{
					Log(key + ": " + val);
				}
				else
				{
					Log(key + ": [snip]");
				}
			});

			Log(c.Request.Method + ": " + c.Request.Scheme + "://" + c.Request.Host + c.Request.Path);
			Log("\n");
			return _next(c);
		}

    }
}
