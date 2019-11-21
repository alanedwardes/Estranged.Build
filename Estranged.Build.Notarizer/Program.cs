using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                .AddSingleton<ExecutableSigner>()
                .BuildServiceProvider();

            provider.GetRequiredService<ILoggerFactory>()
                    .AddConsole();

            var logger = provider.GetRequiredService<ILogger<Program>>();

            var executables = provider.GetRequiredService<ExecutableFinder>().FindExecutables(configuration.AppDirectory).ToArray();

            logger.LogInformation($"Found {executables.Length} binaries: {string.Join(", ", executables.Select(x => x.Name))}");

            var signer = provider.GetRequiredService<ExecutableSigner>();

            foreach (var executable in executables)
            {
                if (configuration.EntitlementsMap.TryGetValue(executable.Name, out string[] entitlements))
                {
                    signer.SignExecutable(configuration.CertificateId, executable, entitlements);
                }
                else
                {
                    signer.SignExecutable(configuration.CertificateId, executable, new string[0]);
                }
            }

            await Task.Delay(1000);
        }
    }
}