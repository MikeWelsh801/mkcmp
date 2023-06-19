namespace Mkcmp.CodeAnalysis.Syntax;

public sealed class ForStatementSyntax : StatementSyntax
{
    public ForStatementSyntax(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken inKeyword,
                              ExpressionSyntax lowerBound, SyntaxToken rangeKeyword, ExpressionSyntax upperBound, 
                              StatementSyntax body)
    {
        Keyword = keyword;
        Identifier = identifier;
        InKeyword = inKeyword;
        LowerBound = lowerBound;
        RangeKeyword = rangeKeyword;
        UpperBound = upperBound;
        Body = body;
    }

    public override SyntaxKind Kind => SyntaxKind.ForStatement;

    public SyntaxToken Keyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken InKeyword { get; }
    public ExpressionSyntax LowerBound { get; }
    public SyntaxToken RangeKeyword { get; }
    public ExpressionSyntax UpperBound { get; }
    public StatementSyntax Body { get; }
}
