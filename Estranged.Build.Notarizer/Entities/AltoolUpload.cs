using System.Runtime.Serialization;

namespace Estranged.Build.Notarizer.Entities
{
    [DataContract]
    internal sealed class AltoolUpload : AltoolOutput
    {
        [DataMember(Name = "notarization-upload")]
        public AltoolNotarizationUpload NotarizationUpload { get; set; }
    }
}
