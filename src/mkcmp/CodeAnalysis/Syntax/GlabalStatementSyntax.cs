namespace Mkcmp.CodeAnalysis.Syntax;

public sealed class GlabalStatementSyntax : MemberSyntax
{
    public GlabalStatementSyntax(StatementSyntax statement)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    public StatementSyntax Statement { get; }
}

