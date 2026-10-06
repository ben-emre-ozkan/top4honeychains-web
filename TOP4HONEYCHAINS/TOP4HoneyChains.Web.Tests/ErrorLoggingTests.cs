using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TOP4HoneyChains.Web.Services;
using Xunit;

namespace TOP4HoneyChains.Web.Tests
{
    // D-67: an unhandled exception is logged with the same short reference the error page shows
    public class ErrorLoggingTests
    {
        private sealed class ListLogger<T> : ILogger<T>
        {
            public readonly List<(LogLevel Level, string Message, Exception? Exception)> Entries = new();
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
                Entries.Add((logLevel, formatter(state, exception), exception));
        }

        [Fact]
        public void The_reference_is_short_and_stable_for_a_request()
        {
            var context = new DefaultHttpContext { TraceIdentifier = "0HZZTEST:00000001" };
            var reference = ErrorReference.Of(context);
            Assert.Matches("^E-[A-Z2-9]{8}$", reference);
            Assert.Equal(reference, ErrorReference.Of(context));
            Assert.NotEqual(reference, ErrorReference.Of(new DefaultHttpContext { TraceIdentifier = "0HZZTEST:00000002" }));
        }

        [Fact]
        public async Task An_unhandled_exception_is_logged_with_the_reference_and_rethrown()
        {
            var logger = new ListLogger<UnhandledExceptionLoggingMiddleware>();
            var boom = new InvalidOperationException("zz test failure");
            var middleware = new UnhandledExceptionLoggingMiddleware(_ => throw boom, logger);
            var context = new DefaultHttpContext { TraceIdentifier = "0HZZTEST:00000003" };
            context.Request.Path = "/zz/test";

            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context));
            Assert.Same(boom, thrown);
            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Error, entry.Level);
            Assert.Same(boom, entry.Exception);
            Assert.Contains(ErrorReference.Of(context), entry.Message);
            Assert.Contains("/zz/test", entry.Message);
        }

        [Fact]
        public void Errors_are_written_to_the_daily_file()
        {
            var folder = Path.Combine(Path.GetTempPath(), "zz_test_errlog_" + Guid.NewGuid().ToString("N"));
            try
            {
                using var provider = new ErrorFileLoggerProvider(folder);
                var logger = provider.CreateLogger("zz.Test");
                logger.LogInformation("not written");
                logger.LogError(new InvalidOperationException("zz boom"), "Unhandled exception {Reference}", "E-ZZTESTAB");
                var text = File.ReadAllText(Directory.GetFiles(folder).Single());
                Assert.Contains("E-ZZTESTAB", text);
                Assert.Contains("zz boom", text);
                Assert.DoesNotContain("not written", text);
            }
            finally
            {
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
            }
        }
    }
}
