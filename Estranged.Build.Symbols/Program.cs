﻿using Amazon.S3;
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

            var s3Config = new AmazonS3Config
            {
                Timeout = TimeSpan.FromHours(6),
                ReadWriteTimeout = TimeSpan.FromHours(6)
            };

            var provider = new ServiceCollection()
                .AddSingleton<SymbolExtractor>()
                .AddSingleton<SymbolUploader>()
                .AddSingleton<ITransferUtility>(new TransferUtility(new AmazonS3Client(s3Config)))
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

            var destination = config["destination"];
            if (!Directory.Exists(destination))
            {
                throw new Exception($"Directory doesn't exist: {destination}");
            }

            string symbolDestination = Path.Combine(symbols, Path.Combine(config["destination"], "ExtractedSymbols-" + Guid.NewGuid()));

            // First extract the symbols
            provider.GetRequiredService<SymbolExtractor>()
                    .ExtractSymbols(symstore, symbols, symbolDestination);

            // Next, upload the symbols to S3
            await provider.GetRequiredService<SymbolUploader>()
                          .UploadSymbols(symbolDestination, config["bucket"], config.GetSection("properties").GetChildren());
        }
    }
}