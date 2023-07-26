namespace Mkcmp.CodeAnalysis.Binding;

internal sealed class BoundWhileStatement : BoundLoopStatement
{
    public BoundWhileStatement(BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
        : base(breakLabel: breakLabel, continueLabel: continueLabel)
    {
        Condition = condition;
        Body = body;
    }

    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
    public BoundExpression Condition { get; }
    public BoundStatement Body { get; }
}
