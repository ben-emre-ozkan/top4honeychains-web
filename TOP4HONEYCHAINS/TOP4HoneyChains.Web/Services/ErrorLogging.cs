using System.Security.Cryptography;
using System.Text;

namespace TOP4HoneyChains.Web.Services
{
    // D-67: one short reference per request, shown on the error page and written next to the exception in the log
    public static class ErrorReference
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";   // no 0/O, 1/I

        public static string Of(HttpContext context)
        {
            if (context.Items.TryGetValue(typeof(ErrorReference), out var existing) && existing is string s)
                return s;
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(context.TraceIdentifier));
            var chars = new char[8];
            for (var i = 0; i < chars.Length; i++)
                chars[i] = Alphabet[hash[i] % Alphabet.Length];
            var reference = "E-" + new string(chars);
            context.Items[typeof(ErrorReference)] = reference;
            return reference;
        }
    }

    // Logs every unhandled exception with the request's reference, in every environment, then lets the exception
    // handler (production) or the developer page (development) answer as before
    public class UnhandledExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UnhandledExceptionLoggingMiddleware> _logger;

        public UnhandledExceptionLoggingMiddleware(RequestDelegate next, ILogger<UnhandledExceptionLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (Log(context, ex))
            {
                throw;   // never reached: Log returns false, so the exception keeps its original stack
            }
        }

        private bool Log(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception {Reference} on {Method} {Path} (user {User})",
                ErrorReference.Of(context), context.Request.Method, context.Request.Path, context.User.Identity?.Name ?? "anonymous");
            return false;
        }
    }

    // Errors (and worse) also go to App_Data/logs/errors-yyyyMMdd.log, so a 500 leaves its stack trace on disk
    // even where the console is not kept (IIS, test runs). No personal data beyond the user name is written.
    public sealed class ErrorFileLoggerProvider : ILoggerProvider
    {
        private readonly string _folder;
        private readonly object _gate = new();

        public ErrorFileLoggerProvider(string folder)
        {
            _folder = folder;
        }

        public ILogger CreateLogger(string categoryName) => new ErrorFileLogger(this, categoryName);

        public void Dispose() { }

        private void Write(string line)
        {
            try
            {
                lock (_gate)
                {
                    Directory.CreateDirectory(_folder);
                    File.AppendAllText(Path.Combine(_folder, $"errors-{DateTime.UtcNow:yyyyMMdd}.log"), line, Encoding.UTF8);
                }
            }
            catch (IOException) { /* logging must never break a request */ }
            catch (UnauthorizedAccessException) { }
        }

        private sealed class ErrorFileLogger : ILogger
        {
            private readonly ErrorFileLoggerProvider _provider;
            private readonly string _category;

            public ErrorFileLogger(ErrorFileLoggerProvider provider, string category)
            {
                _provider = provider;
                _category = category;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Error;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;
                var text = new StringBuilder()
                    .Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("Z ")
                    .Append(logLevel).Append(' ').Append(_category).Append(": ").Append(formatter(state, exception)).AppendLine();
                if (exception != null)
                    text.AppendLine(exception.ToString());
                _provider.Write(text.ToString());
            }
        }
    }
}
