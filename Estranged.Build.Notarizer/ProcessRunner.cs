using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace Estranged.Build.Notarizer
{
    internal sealed class ProcessRunner
    {
        private readonly ILogger<ProcessRunner> logger;

        public ProcessRunner(ILogger<ProcessRunner> logger)
        {
            this.logger = logger;
        }

        public TOutput RunProcess<TOutput>(string executable, string arguments)
        {
            var result = RunProcess(executable, arguments);
            if (result == null)
            {
                throw new Exception($"No output from {executable}.");
            }

            return result.DeserializePlist<TOutput>();
        }

        public void RunShell(string executable)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = executable;
                process.StartInfo.UseShellExecute = true;

                logger.LogInformation($"Starting shell {process.StartInfo.FileName}");

                process.Start();
                process.WaitForExit();
            }
        }

        public string RunProcess(string executable, string arguments)
        {
            using (var process = new Process())
            {
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = executable;
                process.StartInfo.Arguments = arguments;

                logger.LogInformation($"Starting executable {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                process.Start();
                process.WaitForExit();

                var stderr = process.StandardError.ReadToEnd()?.Trim();
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    logger.LogError(stderr);
                }

                var stdout = process.StandardOutput.ReadToEnd()?.Trim();
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    logger.LogInformation(stdout);
                    return stdout;
                }

                return null;
            }
        }
    }
}
