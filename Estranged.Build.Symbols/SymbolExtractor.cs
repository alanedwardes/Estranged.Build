using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Estranged.Build.Symbols
{
    public class SymbolExtractor
    {
        private readonly ILogger<SymbolExtractor> logger;

        public SymbolExtractor(ILogger<SymbolExtractor> logger)
        {
            this.logger = logger;
        }

        public int ExtractSymbols(string symstore, string from, string to)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = symstore,
                Arguments = $"add /f \"{from}\" /s \"{to}\" /t \"{Guid.NewGuid()}\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });

            process.WaitForExit();

            var stderr = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                throw new Exception(stderr);
            }

            var stdout = process.StandardOutput.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(stdout))
            {
                logger.LogInformation(stdout);
            }

            return process.ExitCode;
        }
    }
}
