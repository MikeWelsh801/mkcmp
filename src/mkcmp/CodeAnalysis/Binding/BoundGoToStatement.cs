namespace Mkcmp.CodeAnalysis.Binding;

internal sealed class BoundGoToStatement : BoundStatement
{
    public BoundGoToStatement(BoundLabel label)
    {
        Label = label;
    }

    public override BoundNodeKind Kind => BoundNodeKind.GoToStatement;
    public BoundLabel Label { get; }
}


