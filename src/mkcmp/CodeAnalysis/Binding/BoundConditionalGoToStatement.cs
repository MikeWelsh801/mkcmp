namespace Mkcmp.CodeAnalysis.Binding;

internal sealed class BoundConditionalGoToStatement : BoundStatement
{
    public BoundConditionalGoToStatement(LabelSymbol label, BoundExpression condition, bool jumpIfTrue = true)
    {
        Label = label;
        Condition = condition;
        JumpIfTrue = jumpIfTrue;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGoToStatement;
    public LabelSymbol Label { get; }
    public BoundExpression Condition { get; }
    public bool JumpIfTrue { get; }
}


