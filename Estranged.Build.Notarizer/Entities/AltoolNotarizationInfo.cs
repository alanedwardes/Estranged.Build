using System;
using System.Runtime.Serialization;

namespace Estranged.Build.Notarizer.Entities
{
    [DataContract]
    internal sealed class AltoolNotarizationInfo
    {
        [DataMember(Name = "Date")]
        public DateTimeOffset Date { get; set; }

        [DataMember(Name = "Hash")]
        public string Hash { get; set; }

        [DataMember(Name = "LogFileURL")]
        public Uri LogFileURL { get; set; }

        [DataMember(Name = "RequestUUID")]
        public Guid RequestId { get; set; }

        [DataMember(Name = "Status")]
        public AltoolStatus Status { get; set; }

        [DataMember(Name = "Status Code")]
        public int? StatusCode { get; set; }

        [DataMember(Name = "Status Message")]
        public string StatusMessage { get; set; }
    }
}
