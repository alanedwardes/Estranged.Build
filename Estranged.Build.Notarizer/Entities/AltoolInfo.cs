using System.Runtime.Serialization;

namespace Estranged.Build.Notarizer.Entities
{
    [DataContract]
    internal sealed class AltoolInfo : AltoolOutput
    {
        [DataMember(Name = "notarization-info")]
        public AltoolNotarizationInfo NotarizationInfo { get; set; }
    }
}
