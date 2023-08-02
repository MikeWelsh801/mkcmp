namespace Mkcmp.CodeAnalysis.Syntax;

public sealed class BreakStatementSyntax : StatementSyntax
{
    public BreakStatementSyntax(SyntaxToken keyword)
    {
        Keyword = keyword;
    }

    public override SyntaxKind Kind => SyntaxKind.BreakStatement;
    public SyntaxToken Keyword { get; }
}

