using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Logging;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;

namespace GearUp.Services
{
	public class SimpleErrorPage
	{
		private readonly RequestDelegate _next;
		private ILogger _logger;

		public SimpleErrorPage(RequestDelegate next, ILogger logger)
		{
			if (next == null)
			{
				throw new ArgumentNullException("next");
			}

			_logger = logger;
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (DocumentClientException ex)
			{
				try
				{
					await DisplayException(context, ex);
					return;
				}
				catch (Exception)
				{
				}
				throw;
			}
			catch (Exception ex)
			{
				try
				{
					await DisplayException(context, ex);
					return;
				}
				catch (Exception)
				{
				}
				throw;
			}
		}

		public async Task DisplayException(HttpContext ctx, Exception ex)
		{
			this._logger.WriteError("Exception: " + ex.Message);
			ctx.Response.StatusCode = 500;
			string err = ex.Message;
			if (ex.InnerException != null)
			{
				err = ex.InnerException.Message;
			}

			string json = JsonConvert.SerializeObject(new { Message = err });
			await ctx.Response.WriteAsync(json);
		}

		public async Task DisplayException(HttpContext ctx, DocumentClientException ex)
		{
			this._logger.WriteError("DCE Exception: " + ex.Message);
			ctx.Response.StatusCode = 500;
			string err = ex.Message;
			if (ex.Error != null && ex.Error.Message != null)
			{
				this._logger.WriteError("Has error.message");
				err = ex.Error.Message;
				int startErr = err.IndexOf("Exception =");
                if (startErr > 0)
				{
					err = err.Substring(startErr, err.IndexOf('"', startErr) - startErr);
				}
			}

			string json = JsonConvert.SerializeObject(new { Message = err });
			await ctx.Response.WriteAsync(json);
		}
	}
}