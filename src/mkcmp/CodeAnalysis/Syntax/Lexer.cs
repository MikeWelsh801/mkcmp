using System.Text;
using Mkcmp.CodeAnalysis.Symbols;
using Mkcmp.CodeAnalysis.Text;

namespace Mkcmp.CodeAnalysis.Syntax;

internal sealed class Lexer
{
    private readonly DiagnosticBag _diagnostics = new();
    private readonly SyntaxTree _syntaxTree;
    private readonly SourceText _text;
    private int _position;

    private int _start;
    private SyntaxKind _kind;
    private object _value;

    public Lexer(SyntaxTree syntaxTree)
    {
        _text = syntaxTree.Text;
        _syntaxTree = syntaxTree;
    }

    public DiagnosticBag Diagnostics => _diagnostics;
    private char Current => Peek(0);
    private char Lookahead => Peek(1);

    private char Peek(int offset)
    {
        var index = _position + offset;

        if (index >= _text.Length)
            return '\0';
        return _text[index];
    }

    public SyntaxToken Lex()
    {
        _start = _position;
        _kind = SyntaxKind.BadToken;
        _value = null;

        switch (Current)
        {
            case '\0':
                _kind = SyntaxKind.EndOfFileToken;
                break;
            case '+':
                _kind = SyntaxKind.PlusToken;
                _position++;
                break;
            case '-':
                _kind = SyntaxKind.MinusToken;
                _position++;
                break;
            case '*':
                _kind = SyntaxKind.StarToken;
                _position++;
                break;
            case '/':
                _kind = SyntaxKind.SlashToken;
                _position++;
                break;
            case '(':
                _kind = SyntaxKind.OpenParenToken;
                _position++;
                break;
            case ')':
                _kind = SyntaxKind.CloseParenToken;
                _position++;
                break;
            case '{':
                _kind = SyntaxKind.OpenBraceToken;
                _position++;
                break;
            case '}':
                _kind = SyntaxKind.CloseBraceToken;
                _position++;
                break;
            case ':':
                _kind = SyntaxKind.ColonToken;
                _position++;
                break;
            case ',':
                _kind = SyntaxKind.CommaToken;
                _position++;
                break;
            case '~':
                _kind = SyntaxKind.TildeToken;
                _position++;
                break;
            case '^':
                _kind = SyntaxKind.HatToken;
                _position++;
                break;
            case '&':
                _position++;
                if (Current != '&')
                {
                    _kind = SyntaxKind.AmpersandToken;
                }
                else
                {
                    _kind = SyntaxKind.AmpersandAmpersandToken;
                    _position++;
                }
                break;
            case '|':
                _position++;
                if (Current != '|')
                {
                    _kind = SyntaxKind.PipeToken;
                }
                else
                {
                    _kind = SyntaxKind.PipePipeToken;
                    _position++;
                }
                break;
            case '=':
                _position++;
                if (Current != '=')
                {
                    _kind = SyntaxKind.EqualsToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.EqualsEqualsToken;
                }
                break;
            case '!':
                _position++;
                if (Current != '=')
                {
                    _kind = SyntaxKind.BangToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.BangEqualsToken;
                }
                break;
            case '<':
                _position++;
                if (Current != '=')
                {
                    _kind = SyntaxKind.LessToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.LessOrEqualsToken;
                }
                break;
            case '>':
                _position++;
                if (Current != '=')
                {
                    _kind = SyntaxKind.GreaterToken;
                }
                else
                {
                    _position++;
                    _kind = SyntaxKind.GreaterOrEqualsToken;
                }
                break;
            case '"':
                ReadString();
                break;
            case '0': case '1': case '2': case '3': case '4':
            case '5': case '6': case '7': case '8': case '9':
                ReadNumberToken();
                break;
            case ' ': case '\t': case '\n': case '\r':
                ReadWhiteSpace();
                break;
            default:
                if (char.IsLetter(Current) || Current == '.')
                {
                    ReadIdentifierOrKeyword();
                }
                else if (char.IsWhiteSpace(Current))
                {
                    ReadWhiteSpace();
                }
                else
                {
                    var span = new TextSpan(_position, 1);
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportBadCharacter(location, Current);
                    _position++;
                }
                break;
        }
        var length = _position - _start;
        var text = SyntaxFacts.GetText(_kind);
        if (text == null)
            text = _text.ToString(_start, length);

        return new SyntaxToken(_syntaxTree, _kind, _start, text, _value);
    }

    private void ReadString()
    {
        _position++;
        var sb = new StringBuilder();

        var done = false;
        while (!done)
        {
            switch (Current)
            {
                case '\0':
                case '\r':
                case '\n':
                    var span = new TextSpan(_start, 1);
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportUnterminatedString(location);
                    done = true;
                    break;
                case '"':
                    if (Lookahead == '"')
                    {
                        sb.Append(Current);
                        _position += 2;
                    }
                    else
                    {
                        _position++;
                        done = true;
                    }
                    break;
                default:
                    sb.Append(Current);
                    _position++;
                    break;
            }
        }

        _kind = SyntaxKind.StringToken;
        _value = sb.ToString();
    }

    private void ReadWhiteSpace()
    {
        while (char.IsWhiteSpace(Current))
            _position++;

        _kind = SyntaxKind.WhitespaceToken;
    }

    private void ReadNumberToken()
    {
        while (char.IsDigit(Current))
            _position++;

        var length = _position - _start;
        var text = _text.ToString(_start, length);
        if (!int.TryParse(text, out var value))
        {
            var span = new TextSpan(_start, length);
            var location = new TextLocation(_text, span);
            _diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Int);
        }

        _value = value;
        _kind = SyntaxKind.NumberToken;
    }

    private void ReadIdentifierOrKeyword()
    {
        if (Current == '.')
        {
            while (Current == '.')
                _position++;
            if (Current == '=')
                _position++;
        }
        else
        {
            while (char.IsLetter(Current))
                _position++;
        }

        var length = _position - _start;
        var text = _text.ToString(_start, length);

        _kind = SyntaxFacts.GetKeywordKind(text);
    }
}

