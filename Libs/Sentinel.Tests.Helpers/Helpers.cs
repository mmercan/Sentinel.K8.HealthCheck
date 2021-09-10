using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sentinel.Tests.Helpers
{
    public static class Helpers
    {
        public static ILogger<T> GetLogger<T>()
        {

            var serviceProvider = new ServiceCollection()
           .AddLogging()
           .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            var logger = factory.CreateLogger<T>();
            return logger;
        }
    }
}