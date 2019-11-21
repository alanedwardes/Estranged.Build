using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Estranged.Build.Notarizer
{
    internal sealed class NotarizerConfiguration
    {
        public string AppPath { get; set; }
        public DirectoryInfo AppDirectory => new DirectoryInfo(AppPath);
        public string CertificateId { get; set; }
        public string Entitlements { get; set; }
        public IReadOnlyDictionary<string, string[]> EntitlementsMap => Entitlements.Split(",").Select(x => x.Split("=")).ToDictionary(x => x[0], y => y[1].Split(","));
    }
}
