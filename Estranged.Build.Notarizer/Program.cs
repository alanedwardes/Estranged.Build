using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Estranged.Build.Notarizer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                DoWork(args).Wait();
            }
            catch (AggregateException e)
            {
                throw e.Flatten();
            }
        }

        public static async Task DoWork(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var configuration = new NotarizerConfiguration();
            config.Bind(configuration);

            var provider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ExecutableFinder>()
                .AddSingleton<ExecutableStripper>()
                .AddSingleton<ExecutableSigner>()
                .AddSingleton<ExecutableZipBuilder>()
                .AddSingleton<ExecutableNotarizer>()
                .AddSingleton<ProcessRunner>()
                .AddSingleton<Workflow>()
                .BuildServiceProvider();

            provider.GetRequiredService<ILoggerFactory>()
                    .AddConsole();

            await provider.GetRequiredService<Workflow>().Run(configuration);
        }
    }
}