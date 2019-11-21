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

        public FileInfo BuildZipFile(IEnumerable<FileInfo> executables)
        {
            var zipFile = new FileInfo($"executables-{Guid.NewGuid()}.zip");

            logger.LogInformation($"Building ZIP file {zipFile.Name}");

            using (var fs = zipFile.OpenWrite())
            using (var zs = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                foreach (var executable in executables)
                {
                    logger.LogInformation($"Adding {executable.Name} to zip file");

                    const char pathSeparator = '/';
                    var directory = Guid.NewGuid().ToString().Split("-")[0];
                    zs.CreateEntry(directory + pathSeparator);
                    zs.CreateEntryFromFile(executable.FullName, directory + pathSeparator + executable.Name);
                }
            }

            return zipFile;
        }
    }
}
