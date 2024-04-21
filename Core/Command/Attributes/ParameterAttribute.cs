namespace SilhouetteDance.Core.Command.Attributes;

/// <summary>
/// 其实这个Attribute你不加也行 只不过他会重新Order一下参数在Segment中所在的位置
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal class ParameterAttribute : Attribute
{
    public int Order { get; set; }
    
    public ParameterAttribute(int order) => Order = order;
}