namespace Mkcmp.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // tokens
        BadToken,
        EndOfFileToken,
        WhitespaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        BangToken,
        EqualsToken,
        AmpersandAmpersandToken,
        PipePipeToken,
        BangEqualsToken,
        EqualsEqualsToken,
        OpenParenToken,
        CloseParenToken,
        IdentifierToken,

        // Keywords
        FalseKeyword,
        TrueKeyword,

        // Nodes
        CompilationUnit,

        // Expressions
        LiteralExpression,
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        AssignmentExpression,
    }
}

