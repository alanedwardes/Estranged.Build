using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Estranged.Build.Notarizer
{
    internal sealed class ExecutableZipBuilder
    {
        private readonly ILogger<ExecutableZipBuilder> logger;
        private readonly ProcessRunner processRunner;

        public ExecutableZipBuilder(ILogger<ExecutableZipBuilder> logger, ProcessRunner processRunner)
        {
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public FileInfo BuildZipFile(DirectoryInfo appDirectory, IEnumerable<FileInfo> executables)
        {
            var zipFolder = appDirectory.Parent;
            var zipFile = new FileInfo(Path.Combine(zipFolder.FullName, $"executables-{Guid.NewGuid()}.zip"));

            logger.LogInformation($"Building ZIP file {zipFile.Name}");

            processRunner.RunProcess("/bin/bash", $"-c \"cd {zipFolder.FullName} && ditto -c -k --sequesterRsrc --keepParent {appDirectory.Name} {zipFile.Name}\"");

            return zipFile;
        }
    }
}
