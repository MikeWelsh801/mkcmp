namespace Mkcmp.CodeAnalysis.Syntax;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }

    public BinaryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        : base(syntaxTree)
    {
        Left = left;
        OperatorToken = operatorToken;
        Right = right;
    }
}

