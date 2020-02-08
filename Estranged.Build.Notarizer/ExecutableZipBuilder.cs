using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Estranged.Build.Notarizer
{
    internal sealed class ExecutableZipBuilder
    {
        private readonly ILogger<ExecutableZipBuilder> logger;

        public ExecutableZipBuilder(ILogger<ExecutableZipBuilder> logger)
        {
            this.logger = logger;
        }

        public FileInfo BuildZipFile(DirectoryInfo appDirectory, IEnumerable<FileInfo> executables)
        {
            var zipFile = new FileInfo($"executables-{Guid.NewGuid()}.zip");

            logger.LogInformation($"Building ZIP file {zipFile.Name}");

            var root = appDirectory.FullName.Replace("\\", "/");

            using (var fs = zipFile.OpenWrite())
            using (var zs = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                foreach (var executable in executables)
                {
                    var executablePath = executable.FullName.Replace("\\", "/").Replace(root, string.Empty).Trim('/');

                    logger.LogInformation($"Adding {executablePath} to zip file");
                    var entry = zs.CreateEntryFromFile(executable.FullName, executablePath);
                    entry.ExternalAttributes = -2115174400; // This represents attributes with the executable bit set
                }
            }

            return zipFile;
        }
    }
}
