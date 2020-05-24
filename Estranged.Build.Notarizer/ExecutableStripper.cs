using Microsoft.Extensions.Logging;
using System.IO;

namespace Estranged.Build.Notarizer
{
    internal sealed class ExecutableStripper
    {
        private readonly ILogger<ExecutableSigner> logger;
        private readonly ProcessRunner processRunner;

        public ExecutableStripper(ILogger<ExecutableSigner> logger, ProcessRunner processRunner)
        {
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public void StripExecutable(FileSystemInfo executable)
        {
            processRunner.RunProcess("lipo", $"{executable.FullName} -remove i386 -output {executable.FullName}");
        }
    }
}
