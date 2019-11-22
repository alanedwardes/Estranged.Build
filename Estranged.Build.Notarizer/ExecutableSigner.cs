using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Estranged.Build.Notarizer
{
    internal sealed class ExecutableSigner
    {
        private readonly ILogger<ExecutableSigner> logger;
        private readonly ProcessRunner processRunner;

        public ExecutableSigner(ILogger<ExecutableSigner> logger, ProcessRunner processRunner)
        {
            this.logger = logger;
            this.processRunner = processRunner;
        }

        public void SignExecutable(string certificateId, FileSystemInfo executable, IReadOnlyDictionary<string, string[]> entitlementsMap)
        {
            var entitlements = entitlementsMap.ContainsKey(executable.Name) ? entitlementsMap[executable.Name] : new string[0];

            var sharedArguments = $"--options runtime --timestamp --sign \"{certificateId}\" --force";

            if (entitlements.Length > 0)
            {
                logger.LogInformation($"Signing executable {executable.Name} with entitlements \"{string.Join(",", entitlements)}\"");
                var entitlementsFile = WriteEntitlements(entitlements);
                processRunner.RunProcess("codesign", $"{sharedArguments} --entitlements \"{entitlementsFile.FullName}\" \"{executable.FullName}\"");
                entitlementsFile.Delete();
            }
            else
            {
                logger.LogInformation($"Signing executable {executable.Name} with no entitlements");
                processRunner.RunProcess("codesign", $"{sharedArguments} \"{executable.FullName}\"");
            }

            logger.LogInformation($"Validating signature for executable");
            processRunner.RunProcess("codesign", $"--verify --deep --strict --verbose=2 \"{executable.FullName}\"");
        }

        private FileInfo WriteEntitlements(string[] entitlements)
        {
            var entitlementsFile = new FileInfo($"entitlements-{Guid.NewGuid()}.xml");

            var entitlementsBuilder = new StringBuilder();

            entitlementsBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            entitlementsBuilder.AppendLine("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
            entitlementsBuilder.AppendLine("<plist version=\"1.0\">");
            entitlementsBuilder.AppendLine("<dict>");

            foreach (var entitlement in entitlements)
            {
                entitlementsBuilder.AppendLine($"<key>{entitlement}</key>");
                entitlementsBuilder.AppendLine("<true/>");
            }

            entitlementsBuilder.AppendLine("</dict>");
            entitlementsBuilder.AppendLine("</plist>");

            File.WriteAllText(entitlementsFile.FullName, entitlementsBuilder.ToString());

            logger.LogInformation($"Writing entitlements to {entitlementsFile.Name}");
            return entitlementsFile;
        }
    }
}
