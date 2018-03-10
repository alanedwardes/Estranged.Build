﻿using Amazon;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Estranged.Build.Symbols
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

            var region = RegionEndpoint.GetBySystemName(config["region"]);

            var provider = new ServiceCollection()
                .AddSingleton<SymbolExtractor>()
                .AddSingleton<SymbolUploader>()
                .AddSingleton<ITransferUtility>(x => new TransferUtility(config["accessKey"], config["secret"], region))
                .AddLogging()
                .BuildServiceProvider();

            provider.GetRequiredService<ILoggerFactory>()
                .AddConsole();

            var symbols = config["symbols"];
            if (!Directory.Exists(symbols))
            {
                throw new Exception($"Directory doesn't exist: {symbols}");
            }

            var symstore = config["symstore"];
            if (!File.Exists(symstore))
            {
                throw new Exception($"Executable doesn't exist: {symstore}");
            }

            var extracted = Path.Combine(symbols, "ExtractedSymbols-" + Guid.NewGuid());

            // First extract the symbols
            provider.GetRequiredService<SymbolExtractor>()
                .ExtractSymbols(symstore, symbols, extracted);

            // Next, upload the symbols to S3
            await provider.GetRequiredService<SymbolUploader>()
                .UploadSymbols(extracted, config["bucket"], config.GetSection("properties").GetChildren());
        }
    }
}