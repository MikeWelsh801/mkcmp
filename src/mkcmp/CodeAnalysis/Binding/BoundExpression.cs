using Mkcmp.CodeAnalysis.Symbols;

namespace Mkcmp.CodeAnalysis.Binding;

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }
}
