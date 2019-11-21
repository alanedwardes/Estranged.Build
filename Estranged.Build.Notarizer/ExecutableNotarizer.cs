using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Estranged.Build.Notarizer
{
    internal sealed class ExecutableNotarizer
    {
        private readonly ILogger<ExecutableNotarizer> logger;

        public ExecutableNotarizer(ILogger<ExecutableNotarizer> logger)
        {
            this.logger = logger;
        }

        public async Task NotarizeExecutables(FileInfo zipArchive, DirectoryInfo appDirectory, string developerUsername, string developerPassoword)
        {
            logger.LogInformation($"Notarizing executables in {zipArchive.Name} with developer {developerUsername}");

            using (var process = new Process())
            {
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "xcrun";
                process.StartInfo.Arguments = $"altool --notarize-app -primary-bundle-id \"{zipArchive.Name}\"  -u \"{developerUsername}\" -p \"{developerPassoword}\" --file \"{zipArchive.FullName}\"";

                logger.LogInformation($"Starting executable {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                process.Start();
                process.WaitForExit();

                var stderr = process.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    logger.LogError(stderr);
                }

                var stdout = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    logger.LogInformation(stdout);
                }
            }

            var startWaitTime = DateTime.UtcNow;

            while (true)
            {
                logger.LogInformation("Waiting 1 minute");
                await Task.Delay(TimeSpan.FromMinutes(1));

                var waitedTime = DateTime.UtcNow - startWaitTime;
                if (waitedTime > TimeSpan.FromMinutes(30))
                {
                    logger.LogError("Waited over 30 minutes for notarization");
                }

                using (var process = new Process())
                {
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.FileName = "xcrun";
                    process.StartInfo.Arguments = $"stapler staple \"{appDirectory.FullName}\"";

                    logger.LogInformation($"Starting executable {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                    process.Start();
                    process.WaitForExit();

                    var stderr = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        logger.LogError(stderr);
                    }

                    var stdout = process.StandardOutput.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(stdout))
                    {
                        logger.LogInformation(stdout);
                        break;
                    }
                }
            }
        }
    }
}
