using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Estranged.Build.Notarizer
{
    internal sealed class ExecutableFinder
    {
        private readonly ILogger<ExecutableFinder> logger;

        public ExecutableFinder(ILogger<ExecutableFinder> logger)
        {
            this.logger = logger;
        }

        public FileInfo[] FindExecutables(DirectoryInfo directoryInfo)
        {
            var executables = FindExecutablesInternal(directoryInfo).ToArray();
            logger.LogInformation($"Found {executables.Length} executables: {string.Join(", ", executables.Select(x => x.Name))}");
            return executables;
        }

        private IEnumerable<FileInfo> FindExecutablesInternal(DirectoryInfo directoryInfo)
        {
            var forbidddenFiles = new[] { "CodeResources" };

            foreach (FileInfo file in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(x => !forbidddenFiles.Contains(x.Name)))
            {
                if (file.Extension == ".dylib")
                {
                    yield return file;
                }

                if (file.Extension == string.Empty)
                {
                    if (ContainsBinary(file))
                    {
                        yield return file;
                    }
                }
            }
        }

        private bool ContainsBinary(FileInfo file)
        {
            using (var fs = file.OpenRead())
            using (var br = new BinaryReader(fs))
            {
                while (fs.Position < fs.Length)
                {
                    try
                    {
                        br.ReadChar();
                    }
                    catch (ArgumentException)
                    {
                        // ReadChar breaks if the input is
                        // out of the UTF-8 range - we're
                        // counting on this, as our executable
                        // will be binary (and trigger the exception)
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
