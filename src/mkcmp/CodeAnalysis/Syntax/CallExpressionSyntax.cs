namespace Mkcmp.CodeAnalysis.Syntax;

public sealed class CallExpressionSyntax : ExpressionSyntax
{
    public CallExpressionSyntax(SyntaxToken identifier, SeparatedSyntaxList<ExpressionSyntax> arguments)
    {
        Identifier = identifier;
        Arguments = arguments;
    }

    public override SyntaxKind Kind => SyntaxKind.CallExpression;

    public SyntaxToken Identifier { get; }
    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
}

