using Mkcmp.CodeAnalysis.Syntax;

namespace mkcmp.Tests.CodeAnalysis.Syntax;

public class LexerTest
{
    [Theory]
    [MemberData(nameof(GetTokensData))]
    public void Lexer_Lexes_Token(SyntaxKind kind, string text)
    {
        var tokens = SyntaxTree.ParseTokens(text);

        var token = Assert.Single(tokens);
        Assert.Equal(kind, token.Kind);
        Assert.Equal(text, token.Text);
    }

    public static IEnumerable<object[]> GetTokensData()
    {
        foreach (var t in GetTokens())
            yield return new object[] { t.kind, t.text };
    }

    private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
    {
        return new[]
        {
            (SyntaxKind.WhitespaceToken, " "),
            (SyntaxKind.WhitespaceToken, "  "),
            (SyntaxKind.WhitespaceToken, "\r"),
            (SyntaxKind.WhitespaceToken, "\n"),
            (SyntaxKind.WhitespaceToken, "\r\n"),
            
            (SyntaxKind.NumberToken, "1"),
            (SyntaxKind.NumberToken, "12"),
            (SyntaxKind.NumberToken, "123"),

            (SyntaxKind.PlusToken, "+"),
            (SyntaxKind.MinusToken, "-"),
            (SyntaxKind.StarToken, "*"),
            (SyntaxKind.SlashToken, "/"),
            (SyntaxKind.BangToken, "!"),
            (SyntaxKind.EqualsToken, "="),
            (SyntaxKind.AmpersandAmpersandToken, "&&"),
            (SyntaxKind.PipePipeToken, "||"),
            (SyntaxKind.BangEqualsToken, "!="),
            (SyntaxKind.EqualsEqualsToken, "=="),
            (SyntaxKind.OpenParenToken, "("),
            (SyntaxKind.CloseParenToken, ")"),

            (SyntaxKind.IdentifierToken, "a"),
            (SyntaxKind.IdentifierToken, "abc"),
            (SyntaxKind.FalseKeyword, "false"),
            (SyntaxKind.TrueKeyword, "true"),
        };


    }
}
