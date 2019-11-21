using System.Linq;
using System.Threading.Tasks;

namespace Estranged.Build.Notarizer
{
    internal sealed class Workflow
    {
        private readonly ExecutableFinder executableFinder;
        private readonly ExecutableSigner executableSigner;
        private readonly ExecutableZipBuilder executableZipBuilder;
        private readonly ExecutableNotarizer executableNotarizer;

        public Workflow(ExecutableFinder executableFinder, ExecutableSigner executableSigner, ExecutableZipBuilder executableZipBuilder, ExecutableNotarizer executableNotarizer)
        {
            this.executableFinder = executableFinder;
            this.executableSigner = executableSigner;
            this.executableZipBuilder = executableZipBuilder;
            this.executableNotarizer = executableNotarizer;
        }

        public async Task Run(NotarizerConfiguration configuration)
        {
            var executables = executableFinder.FindExecutables(configuration.AppDirectory).ToArray();

            foreach (var executable in executables)
            {
                executableSigner.SignExecutable(configuration.CertificateId, executable, configuration.EntitlementsMap);
            }

            executableSigner.SignExecutable(configuration.CertificateId, configuration.AppDirectory, configuration.EntitlementsMap);

            var zipFile = executableZipBuilder.BuildZipFile(configuration.AppDirectory, executables);

            await executableNotarizer.NotarizeExecutables(zipFile, configuration.AppDirectory, configuration.DeveloperUsername, configuration.DeveloperPassword);
        }
    }
}
