using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psncrawler
{
    public interface ILogger
    {
        Task DebugAsync(string message);
        Task InfoAsync(string message);
        Task WarningAsync(string message);
        Task ErrorAsync(string message);
        Task ExceptionAsync(Exception ex);
    }

    public interface ILoggerHandler
    {
        Task LogAsync(int severity, DateTime dateTime, string message);
    }

    public class ActionLogger : ILoggerHandler
    {
        private readonly Func<int, DateTime, string, Task> _action;

        public ActionLogger(Func<int, DateTime, string, Task> action)
        {
            _action = action;
        }

        public Task LogAsync(int severity, DateTime dateTime, string message) =>
            _action.Invoke(severity, dateTime, message);
    }

    public class ConcurrencyLogger : ILoggerHandler
    {
        private readonly object _locker = new object();
        private readonly ILoggerHandler _innerLoggerHandler;

        public ConcurrencyLogger(ILoggerHandler innerLoggerHandler)
        {
            _innerLoggerHandler = innerLoggerHandler;
        }

        public Task LogAsync(int severity, DateTime dateTime, string message)
        {
            lock (_locker)
                return _innerLoggerHandler.LogAsync(severity, dateTime, message);
        }
    }

    public class Logger : ILogger
    {
        private readonly IEnumerable<ILoggerHandler> _loggerHandlers;

        public Logger(IEnumerable<ILoggerHandler> loggerHandlers)
        {
            _loggerHandlers = loggerHandlers;
        }

        public Task DebugAsync(string message) => LogInternal(0, $"DBG {message}");
        public Task InfoAsync(string message) => LogInternal(1, $"INF {message}");
        public Task WarningAsync(string message) => LogInternal(2, $"WRN {message}");
        public Task ErrorAsync(string message) => LogInternal(3, $"ERR {message}");
        public Task ExceptionAsync(Exception ex) => ErrorAsync($"{ex.Message}\n-----\n{ex.Source}\n{ex.StackTrace}\n-----");

        private Task LogInternal(int severity, string message)
        {
            var dateTime = DateTime.UtcNow;
            var tasks = _loggerHandlers.Select(handler => handler.LogAsync(severity, dateTime, message));

            return Task.WhenAll(tasks);
        }
    }
}