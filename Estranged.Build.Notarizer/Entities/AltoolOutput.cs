using System.Runtime.Serialization;

namespace Estranged.Build.Notarizer.Entities
{
    [DataContract]
    internal abstract class AltoolOutput
    {
        [DataMember(Name = "os-version")]
        public string OsVersion { get; set; }

        [DataMember(Name = "success-message")]
        public string SuccessMessage { get; set; }

        [DataMember(Name = "tool-path")]
        public string ToolPath { get; set; }

        [DataMember(Name = "tool-version")]
        public string ToolVersion { get; set; }
    }
}
