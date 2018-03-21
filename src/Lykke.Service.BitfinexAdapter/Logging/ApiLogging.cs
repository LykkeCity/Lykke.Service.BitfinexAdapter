using Microsoft.Extensions.Logging;

namespace Lykke.Service.BitfinexAdapter.Logging
{
    public class ApiLogging
    {
        public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory()
            .AddConsole(LogLevel.Debug);

        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}
