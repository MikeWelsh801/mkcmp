namespace Mkcmp.CodeAnalysis.Syntax;

public static class SyntaxFacts
{
    public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.BangToken:
            case SyntaxKind.TildeToken:
                return 6;

            default:
                return 0;
        }
    }

    public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind.StarToken:
            case SyntaxKind.SlashToken:
                return 5;

            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
                return 4;

            case SyntaxKind.EqualsEqualsToken:
            case SyntaxKind.BangEqualsToken:
            case SyntaxKind.LessToken:
            case SyntaxKind.GreaterToken:
            case SyntaxKind.LessOrEqualsToken:
            case SyntaxKind.GreaterOrEqualsToken:
                return 3;

            case SyntaxKind.AmpersandToken:
            case SyntaxKind.AmpersandAmpersandToken:
                return 2;

            case SyntaxKind.PipeToken:
            case SyntaxKind.PipePipeToken:
            case SyntaxKind.HatToken:
                return 1;

            default:
                return 0;
        }
    }

    public static SyntaxKind GetKeywordKind(string text)
    {
        return text switch
        {
            "break" => SyntaxKind.BreakKeyword,
            "continue" => SyntaxKind.ContinueKeyword,
            "else" => SyntaxKind.ElseKeyword,
            "false" => SyntaxKind.FalseKeyword,
            "for" => SyntaxKind.ForKeyword,
            "fun" => SyntaxKind.FunctionKeyword,
            "if" => SyntaxKind.IfKeyword,
            "in" => SyntaxKind.InKeyword,
            "let" => SyntaxKind.LetKeyword,
            "return" => SyntaxKind.ReturnKeyword,
            "true" => SyntaxKind.TrueKeyword,
            "var" => SyntaxKind.VarKeyword,
            "while" => SyntaxKind.WhileKeyword,
            "do" => SyntaxKind.DoKeyword,
            ".." => SyntaxKind.ToKeyword,
            "..=" => SyntaxKind.ThroughKeyword,
            _ => SyntaxKind.IdentifierToken
        };
    }

    public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
    {
        var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var kind in kinds)
        {
            if (GetUnaryOperatorPrecedence(kind) > 0)
                yield return kind;
        }
    }

    public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
    {
        var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var kind in kinds)
        {
            if (GetBinaryOperatorPrecedence(kind) > 0)
                yield return kind;
        }
    }

    public static string GetText(SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind.PlusToken:
                return "+";
            case SyntaxKind.MinusToken:
                return "-";
            case SyntaxKind.StarToken:
                return "*";
            case SyntaxKind.SlashToken:
                return "/";
            case SyntaxKind.BangToken:
                return "!";
            case SyntaxKind.EqualsToken:
                return "=";
            case SyntaxKind.TildeToken:
                return "~";
            case SyntaxKind.LessToken:
                return "<";
            case SyntaxKind.GreaterToken:
                return ">";
            case SyntaxKind.AmpersandToken:
                return "&";
            case SyntaxKind.AmpersandAmpersandToken:
                return "&&";
            case SyntaxKind.PipeToken:
                return "|";
            case SyntaxKind.PipePipeToken:
                return "||";
            case SyntaxKind.HatToken:
                return "^";
            case SyntaxKind.BangEqualsToken:
                return "!=";
            case SyntaxKind.EqualsEqualsToken:
                return "==";
            case SyntaxKind.LessOrEqualsToken:
                return "<=";
            case SyntaxKind.GreaterOrEqualsToken:
                return ">=";
            case SyntaxKind.OpenParenToken:
                return "(";
            case SyntaxKind.CloseParenToken:
                return ")";
            case SyntaxKind.OpenBraceToken:
                return "{";
            case SyntaxKind.CloseBraceToken:
                return "}";
            case SyntaxKind.ColonToken:
                return ":";
            case SyntaxKind.CommaToken:
                return ",";
            case SyntaxKind.BreakKeyword:
                return "break";
            case SyntaxKind.ContinueKeyword:
                return "continue";
            case SyntaxKind.ElseKeyword:
                return "else";
            case SyntaxKind.FalseKeyword:
                return "false";
            case SyntaxKind.ForKeyword:
                return "for";
            case SyntaxKind.FunctionKeyword:
                return "fun";
            case SyntaxKind.IfKeyword:
                return "if";
            case SyntaxKind.InKeyword:
                return "in";
            case SyntaxKind.LetKeyword:
                return "let";
            case SyntaxKind.ReturnKeyword:
                return "return";
            case SyntaxKind.TrueKeyword:
                return "true";
            case SyntaxKind.VarKeyword:
                return "var";
            case SyntaxKind.WhileKeyword:
                return "while";
            case SyntaxKind.DoKeyword:
                return "do";
            case SyntaxKind.ToKeyword:
                return "..";
            case SyntaxKind.ThroughKeyword:
                return "..=";
            default:
                return null;
        }

    }
}


