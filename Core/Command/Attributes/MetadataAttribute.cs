namespace SilhouetteDance.Core.Command.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class MetadataAttribute : Attribute
{
    public MetadataAttribute(MetadataType type)
    {
        Type = type;
    }
    public MetadataType Type { get; }
    public enum MetadataType
    {
        Uin,
        Timestamp,
        OriginalMessage
    }
}