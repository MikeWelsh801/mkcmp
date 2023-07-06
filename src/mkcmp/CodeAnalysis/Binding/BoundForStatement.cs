using Mkcmp.CodeAnalysis.Symbols;
using Mkcmp.CodeAnalysis.Syntax;

namespace Mkcmp.CodeAnalysis.Binding;

internal sealed class BoundForStatement : BoundStatement
{
    public BoundForStatement(VariableSymbol variable, BoundExpression lowerBound, 
                             SyntaxToken rangeKeyword, BoundExpression upperBound, 
                             BoundStatement body)
    {
        Variable = variable;
        LowerBound = lowerBound;
        RangeKeyword = rangeKeyword;
        UpperBound = upperBound;
        Body = body;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
    public VariableSymbol Variable { get; }
    public BoundExpression LowerBound { get; }
    public SyntaxToken RangeKeyword { get; }
    public BoundExpression UpperBound { get; }
    public BoundStatement Body { get; }
}

