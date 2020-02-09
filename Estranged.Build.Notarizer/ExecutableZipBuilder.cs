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

            // Translate each executable to be relative to the zip path
            var relativeExecutables = executables.Select(x => x.FullName.Replace(appDirectory.FullName, string.Empty))
                .Select(x => appDirectory.Name + x);

            processRunner.RunShell($"cd {zipFolder.FullName} && zip {zipFile.Name} {string.Join(" ", relativeExecutables.Select(x => $"'{x}'"))}");

            return zipFile;
        }
    }
}
