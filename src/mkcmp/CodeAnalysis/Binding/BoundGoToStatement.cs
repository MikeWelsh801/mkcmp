using mkcmp.codeanalysis;

namespace Mkcmp.CodeAnalysis.Binding;

internal sealed class BoundGoToStatement : BoundStatement
{
    public BoundGoToStatement(LabelSymbol label)
    {
        Label = label;
    }

    public override BoundNodeKind Kind => BoundNodeKind.GoToStatement;
    public LabelSymbol Label { get; }
}


