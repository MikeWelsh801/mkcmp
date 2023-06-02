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

    [Theory]
    [MemberData(nameof(GetTokenPairsData))]
    public void Lexer_Lexes_Token_Pairs(SyntaxKind t1kind, string t1text,
                                        SyntaxKind t2kind, string t2text)
    {
        var text = t1text + t2text;
        var tokens = SyntaxTree.ParseTokens(text).ToArray();

        Assert.Equal(2, tokens.Length);
        Assert.Equal(t1kind, tokens[0].Kind);
        Assert.Equal(t1text, tokens[0].Text);
        Assert.Equal(t2kind, tokens[1].Kind);
        Assert.Equal(t2text, tokens[1].Text);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairsWithSeparatorData))]
    public void Lexer_Lexes_Token_Pairs_With_Separator(SyntaxKind t1kind, string t1text,
                                                       SyntaxKind separatorKind, string separatorText,
                                                       SyntaxKind t2kind, string t2text)
    {
        var text = t1text + separatorText + t2text;
        var tokens = SyntaxTree.ParseTokens(text).ToArray();

        Assert.Equal(3, tokens.Length);
        Assert.Equal(t1kind, tokens[0].Kind);
        Assert.Equal(t1text, tokens[0].Text);
        Assert.Equal(separatorKind, tokens[1].Kind);
        Assert.Equal(separatorText, tokens[1].Text);
        Assert.Equal(t2kind, tokens[2].Kind);
        Assert.Equal(t2text, tokens[2].Text);
    }

    public static IEnumerable<object[]> GetTokensData()
    {
        foreach (var t in GetTokens().Concat(GetSeparators()))
            yield return new object[] { t.kind, t.text };
    }

    public static IEnumerable<object[]> GetTokenPairsData()
    {
        foreach (var t in GetTokenPairs())
            yield return new object[] { t.t1kind, t.t1text, t.t2kind, t.t2text };
    }

    public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
    {
        foreach (var t in GetTokenPairsWithSeparator())
            yield return new object[] { t.t1kind, t.t1text,
                                        t.separatorKind, t.separatorText,
                                        t.t2kind, t.t2text };
    }

    private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
    {
        return new[]
        {
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
            (SyntaxKind.NumberToken, "1"),
            (SyntaxKind.NumberToken, "12"),
            (SyntaxKind.NumberToken, "123"),
        };
    }

    private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
    {
        return new[]
        {
            (SyntaxKind.WhitespaceToken, " "),
            (SyntaxKind.WhitespaceToken, "  "),
            (SyntaxKind.WhitespaceToken, "\r"),
            (SyntaxKind.WhitespaceToken, "\n"),
            (SyntaxKind.WhitespaceToken, "\r\n")
        };
    }

    private static bool RequiresSeparator(SyntaxKind t1kind, SyntaxKind t2kind)
    {
        var t1IsKeyword = t1kind.ToString().EndsWith("Keyword");
        var t2IsKeyword = t2kind.ToString().EndsWith("Keyword");

        if (t1kind == SyntaxKind.IdentifierToken && t2kind == SyntaxKind.IdentifierToken)
            return true;
        if (t1IsKeyword && t2IsKeyword)
            return true;
        if (t1IsKeyword && t2kind == SyntaxKind.IdentifierToken)
            return true;
        if (t1kind == SyntaxKind.IdentifierToken && t2IsKeyword)
            return true;
        if (t1kind == SyntaxKind.NumberToken && t2kind == SyntaxKind.NumberToken)
            return true;
        if (t1kind == SyntaxKind.BangToken && t2kind == SyntaxKind.EqualsToken)
            return true;
        if (t1kind == SyntaxKind.EqualsToken && t2kind == SyntaxKind.EqualsToken)
            return true;
        if (t1kind == SyntaxKind.EqualsToken && t2kind == SyntaxKind.EqualsEqualsToken)
            return true;
        if (t1kind == SyntaxKind.BangToken && t2kind == SyntaxKind.EqualsEqualsToken)
            return true;

        // TODO: More cases

        return false;
    }

    private static IEnumerable<(SyntaxKind t1kind, string t1text, SyntaxKind t2kind, string t2text)> GetTokenPairs()
    {
        foreach (var t1 in GetTokens())
            foreach (var t2 in GetTokens())
            {
                if (!RequiresSeparator(t1.kind, t2.kind))
                    yield return (t1.kind, t1.text, t2.kind, t2.text);
            }
    }

    private static IEnumerable<(SyntaxKind t1kind, string t1text,
                                SyntaxKind separatorKind, string separatorText,
                                SyntaxKind t2kind, string t2text)> GetTokenPairsWithSeparator()
    {
        foreach (var t1 in GetTokens())
            foreach (var t2 in GetTokens())
            {
                if (RequiresSeparator(t1.kind, t2.kind))
                    foreach (var separator in GetSeparators())
                        yield return (t1.kind, t1.text, separator.kind, separator.text,
                                      t2.kind, t2.text);
            }
    }
}
