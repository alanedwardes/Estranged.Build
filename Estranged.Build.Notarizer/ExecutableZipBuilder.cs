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
            var zipFile = new FileInfo($"executables-{Guid.NewGuid()}.zip");

            logger.LogInformation($"Building ZIP file {zipFile.Name}");

            var executablesEscaped = executables.Select(x => $"'{x}'");

            processRunner.RunProcess("zip", $"{zipFile.Name} {string.Join(" ", executablesEscaped)}", appDirectory.Parent);

            return zipFile;
        }
    }
}
