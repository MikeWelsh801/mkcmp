namespace Mkcmp.CodeAnalysis.Binding;

internal abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }

    // public IEnumerable<BoundNode> GetChildren()
    // {
    //     var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
    // 
    //     foreach (var property in properties)
    //     {
    //         if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
    //         {
    //             var child = property.GetValue(this) as BoundNode;
    //             if (child is not null)
    //                 yield return child;
    //         }
    //         else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
    //         {
    //             var values = property.GetValue(this) as IEnumerable<BoundNode>;
    //             if (values is not null)
    //                 foreach (var child in values)
    //                     yield return child;
    //         }
    //     }
    // }
    // 
    // private IEnumerable<(string Name, object Value)> GetProperties()
    // {
    //     var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
    // 
    //     foreach (var property in properties)
    //     {
    //         if (property.Name == nameof(Kind) ||
    //             property.Name == nameof(BoundBinaryExpression.Op))
    //             continue;
    // 
    //         if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
    //             typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
    //             continue;
    // 
    //         var value = property.GetValue(this);
    //         if (value is not null)
    //             yield return (property.Name, value);
    //     }
    // }

    public override string ToString()
    {
        using var writer = new StringWriter();
        this.WriteTo(writer);
        return writer.ToString();
    }
}

