using System;
using System.Runtime.Serialization;

namespace Estranged.Build.Notarizer.Entities
{
    [DataContract]
    internal sealed class AltoolNotarizationUpload
    {
        [DataMember(Name = "RequestUUID")]
        public Guid? RequestId { get; set; }
    }
}
