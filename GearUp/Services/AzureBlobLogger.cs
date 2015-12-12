namespace GearUp.Services
{
	using Microsoft.Extensions.Logging;
	using Microsoft.WindowsAzure.Storage;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Threading.Tasks;

	public class AzureBlobLogger : ILogger
    {
        private readonly string _name;
        private readonly object _lock = new object();
        private readonly CloudStorageAccount _storageAccount;
        private readonly string _blobContainer;
        private StringBuilder _log;
        private DateTime _flushTime;

        private static readonly Dictionary<Microsoft.Extensions.Logging.LogLevel, string> _logLevelMappings = new Dictionary<Microsoft.Extensions.Logging.LogLevel, string>()
        {
            { Microsoft.Extensions.Logging.LogLevel.Information, "info" },
            { Microsoft.Extensions.Logging.LogLevel.Critical, "critical" },
            { Microsoft.Extensions.Logging.LogLevel.Debug, "debug" },
            { Microsoft.Extensions.Logging.LogLevel.Error, "error" },
            { Microsoft.Extensions.Logging.LogLevel.Trace, "trace" },
            { Microsoft.Extensions.Logging.LogLevel.Warning, "warning" }
        };
        
        private async Task uploadLog(string str)
        {
            var client = _storageAccount.CreateCloudBlobClient();
			var container = client.GetContainerReference(_blobContainer);
            await container.CreateIfNotExistsAsync();
            var filename = string.Format("{0:yyyy-MM-dd HH:mm:ss}.log", DateTime.Now);
			var blob = container.GetBlockBlobReference(filename);
			await blob.UploadTextAsync(str);
			blob.Properties.ContentType = "text/plain";
			await blob.SetPropertiesAsync();
            
        }
        private void flushLog()
        {
            resetFlushTimer();
            var result = _log.ToString();
            Task.Run(async delegate {
                await uploadLog(result);
            });
            
            _log.Clear();
        }

        private void writeToLog(string msg)
        {
            _log.AppendLine(msg);
            if (DateTime.Now > _flushTime)
            {
                flushLog();
            }
        }
        
        private void resetFlushTimer()
        {
            _flushTime = DateTime.Now.Add(TimeSpan.FromMinutes(5));
        }


        public AzureBlobLogger(string name, string blobStorageConnectionString, string containerName)
        {
            _name = name;
            _storageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
            _blobContainer = containerName;
            _log = new StringBuilder();
            resetFlushTimer();
        }

        protected string Name { get { return _name; } }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel lev)
        {
            return true;
        }

        public void Log(Microsoft.Extensions.Logging.LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var message = string.Empty;
            var values = state as ILogValues;
            if (formatter != null)
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



            lock (_lock)
            {
                var str = string.Format("{0:yyyy-MM-dd HH:mm:ss} ", DateTime.Now) + _logLevelMappings[logLevel] + ": " + message;
                writeToLog(str);
            }
        }

        public IDisposable BeginScopeImpl(object state)
        {
            return null;
        }

    }
}

