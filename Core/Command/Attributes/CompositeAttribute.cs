namespace SilhouetteDance.Core.Command.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class CompositeAttribute : Attribute
{
    public int? Count;

    public CompositeAttribute() { }

    public CompositeAttribute(int count) => Count = count;
}