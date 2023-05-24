namespace Mkcmp.CodeAnalysis
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
        OpenParenToken,
        CloseParenToken,

        // Expressions
        NumberExpression,
        BinaryExpression,
        ParenthesizedExpression
    }
}

