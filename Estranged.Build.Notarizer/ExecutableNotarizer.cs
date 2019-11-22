using Estranged.Build.Notarizer.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Estranged.Build.Notarizer
{
    internal sealed class ExecutableNotarizer
    {
        private readonly ILogger<ExecutableNotarizer> logger;
        private readonly ProcessRunner processRunner;

        public ExecutableNotarizer(ILogger<ExecutableNotarizer> logger, ProcessRunner processRunner)
        {
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public async Task NotarizeExecutables(FileInfo zipArchive, DirectoryInfo appDirectory, string developerUsername, string developerPassword)
        {
            logger.LogInformation($"Notarizing executables in {zipArchive.Name} with developer {developerUsername}");

            logger.LogInformation($"Submitting {zipArchive.Name} to Apple");

            var sharedArguments = $"--username \"{developerUsername}\" --password \"{developerPassword}\" --output-format xml";

            var upload = processRunner.RunProcess<AltoolUpload>("xcrun", $"altool {sharedArguments} --notarize-app -primary-bundle-id \"{zipArchive.Name}\" --file \"{zipArchive.FullName}\"");
            if (upload.NotarizationUpload?.RequestId == null)
            {
                throw new Exception("Didn't get a request ID from the notarization service.");
            }

            logger.LogInformation($"Got request ID {upload.NotarizationUpload.RequestId.Value}");

            var startWaitTime = DateTime.UtcNow;

            AltoolInfo info;
            do
            {
                logger.LogInformation("Waiting 1 minute");
                await Task.Delay(TimeSpan.FromMinutes(1));

                var waitedTime = DateTime.UtcNow - startWaitTime;
                if (waitedTime > TimeSpan.FromMinutes(30))
                {
                    throw new Exception("Waited over 30 minutes for notarization");
                }

                info = processRunner.RunProcess<AltoolInfo>("xcrun", $"altool {sharedArguments} --notarization-info {upload.NotarizationUpload.RequestId.Value}");
            }
            while (info.NotarizationInfo.Status == AltoolStatus.InProgress);

            logger.LogInformation($"Status message: {info.NotarizationInfo?.StatusMessage}");
            logger.LogInformation($"Log file URL: {info.NotarizationInfo?.LogFileURL}");

            if (info.NotarizationInfo.Status != AltoolStatus.Success)
            {
                throw new Exception("Notarization was not successful. See log for details.");
            }

            processRunner.RunProcess("xcrun", $"stapler staple \"{appDirectory.FullName}\"");

            logger.LogInformation("Testing whether gatekeeper would run the app");
            processRunner.RunProcess("xcrun", $"spctl -vvv --assess --type exec \"{appDirectory.FullName}\"");
        }
    }
}
