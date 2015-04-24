using Microsoft.Framework.Logging;
using System;
using System.Collections;
using System.Text;

//fixme get rid of this crap when the next aspnet beta comes out


namespace GearUp.Services
{
	public class TraceProvider : ILoggerProvider
	{
		ILogger ILoggerProvider.Create(string name)
		{
			return new TraceLogger(name);
		}

		ILogger Create(string name)
		{
			return new TraceLogger(name);
		}

		private class TraceLogger : ILogger
		{
			private string name;
			private const int _indentation = 2;

			public TraceLogger(string name)
			{
				this.name = name;
			}

			public IDisposable BeginScope(object state)
			{
				return null;
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				//throw new NotImplementedException();
				return true;
			}

			public void Write(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
			{
				var message = string.Empty;
				var structure = state as ILoggerStructure;
				if (structure != null)
				{
					var builder = new StringBuilder();
					FormatLoggerStructure(
						builder,
						structure,
						level: 1,
						bullet: false);
					message = builder.ToString();
					if (exception != null)
					{
						message += Environment.NewLine + exception;
					}
				}
				else if (formatter != null)
				{
					message = formatter(state, exception);
				}
				else
				{
					message = LogFormatter.Formatter(state, exception);
				}
				if (string.IsNullOrEmpty(message))
				{
					return;
				}

				var severity = logLevel.ToString().ToUpperInvariant();

                System.Diagnostics.Trace.TraceInformation("[{0}:{1}] {2}",
					severity, this.name, message);
			}
			private void FormatLoggerStructure(StringBuilder builder, ILoggerStructure structure, int level, bool bullet)
			{
				if (structure.Message != null)
				{
					builder.Append(structure.Message);
				}
				var values = structure.GetValues();
				if (values == null)
				{
					return;
				}
				var isFirst = true;
				foreach (var kvp in values)
				{
					builder.AppendLine();
					if (bullet && isFirst)
					{
						builder.Append(' ', level * _indentation - 1)
							   .Append('-');
					}
					else
					{
						builder.Append(' ', level * _indentation);
					}
					builder.Append(kvp.Key)
						   .Append(": ");
					if (kvp.Value is IEnumerable && !(kvp.Value is string))
					{
						foreach (var value in (IEnumerable)kvp.Value)
						{
							if (value is ILoggerStructure)
							{
								FormatLoggerStructure(
									builder,
									(ILoggerStructure)value,
									level + 1,
									bullet: true);
							}
							else
							{
								builder.AppendLine()
									   .Append(' ', (level + 1) * _indentation)
									   .Append(value);
							}
						}
					}
					else if (kvp.Value is ILoggerStructure)
					{
						FormatLoggerStructure(
							builder,
							(ILoggerStructure)kvp.Value,
							level + 1,
							bullet: false);
					}
					else
					{
						builder.Append(kvp.Value);
					}
					isFirst = false;
				}
			}
	}
	}
}