using System.Runtime.Serialization;

namespace Estranged.Build.Notarizer.Entities
{
    [DataContract]
    public enum AltoolStatus
    {
        [EnumMember(Value = "in progress")]
        InProgress,
        [EnumMember(Value = "success")]
        Success,
        [EnumMember(Value = "invalid")]
        Invalid
    }
}
