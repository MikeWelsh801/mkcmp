namespace Mkcmp.CodeAnalysis.Syntax;

public sealed class CallExpressionSyntax : ExpressionSyntax
{
    public CallExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, SyntaxToken openParanthesisToken, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closeParenthesisToken)
        : base(syntaxTree)
    {
        Identifier = identifier;
        OpenParanthesisToken = openParanthesisToken;
        Arguments = arguments;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public override SyntaxKind Kind => SyntaxKind.CallExpression;
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenParanthesisToken { get; }
    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
    public SyntaxToken CloseParenthesisToken { get; }
}

