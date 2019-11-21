using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Estranged.Build.Notarizer
{
    internal sealed class ExecutableSigner
    {
        private readonly ILogger<ExecutableSigner> logger;

        public ExecutableSigner(ILogger<ExecutableSigner> logger)
        {
            this.logger = logger;
        }

        public void SignExecutable(string certificateId, FileSystemInfo executable, IReadOnlyDictionary<string, string[]> entitlementsMap)
        {
            var entitlements = entitlementsMap.ContainsKey(executable.Name) ? entitlementsMap[executable.Name] : new string[0];

            logger.LogInformation($"Signing executable {executable.Name} with entitlements \"{string.Join(",", entitlements)}\"");

            var entitlementsFile = WriteEntitlements(entitlements);

            using (var process = new Process())
            {
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "codesign";
                process.StartInfo.Arguments = $"--options runtime --timestamp --sign \"{certificateId}\" --entitlements \"{entitlementsFile.FullName}\" --force \"{executable.FullName}\"";

                logger.LogInformation($"Starting executable {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                process.Start();
                process.WaitForExit();

                var stderr = process.StandardError.ReadToEnd()?.Trim();
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    logger.LogError(stderr);
                }

                var stdout = process.StandardOutput.ReadToEnd()?.Trim();
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    logger.LogInformation(stdout);
                }
            }

            entitlementsFile.Delete();
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
