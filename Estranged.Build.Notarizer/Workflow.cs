using System.Linq;
using System.Threading.Tasks;

namespace Estranged.Build.Notarizer
{
    internal sealed class Workflow
    {
        private readonly ExecutableFinder executableFinder;
        private readonly ExecutableStripper executableStripper;
        private readonly ExecutableSigner executableSigner;
        private readonly ExecutableZipBuilder executableZipBuilder;
        private readonly ExecutableNotarizer executableNotarizer;

        public Workflow(ExecutableFinder executableFinder, ExecutableStripper executableStripper, ExecutableSigner executableSigner, ExecutableZipBuilder executableZipBuilder, ExecutableNotarizer executableNotarizer)
        {
            this.executableFinder = executableFinder;
            this.executableStripper = executableStripper;
            this.executableSigner = executableSigner;
            this.executableZipBuilder = executableZipBuilder;
            this.executableNotarizer = executableNotarizer;
        }

        public async Task Run(NotarizerConfiguration configuration)
        {
            var executables = executableFinder.FindExecutables(configuration.AppDirectory).ToArray();

            foreach (var executable in executables.Where(x => x.Name.EndsWith(".dylib")))
            {
                executableStripper.StripExecutable(executable);
            }

            foreach (var executable in executables)
            {
                executableSigner.SignExecutable(configuration.CertificateId, executable, configuration.EntitlementsMap);
            }

            executableSigner.SignExecutable(configuration.CertificateId, configuration.AppDirectory, configuration.EntitlementsMap);

            if (!configuration.SkipNotarization)
            {
                var zipFile = executableZipBuilder.BuildZipFile(configuration.AppDirectory, executables);

                await executableNotarizer.NotarizeExecutables(zipFile, configuration.AppDirectory, configuration.DeveloperUsername, configuration.DeveloperPassword);

                zipFile.Delete();
            }
        }
    }
}
